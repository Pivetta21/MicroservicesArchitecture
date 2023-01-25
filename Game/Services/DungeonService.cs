using System.Diagnostics;
using AutoMapper;
using Common.DTOs.Item;
using Common.DTOs.PlayDungeon;
using FluentResults;
using Game.AsyncDataServices;
using Game.Data;
using Game.Models;
using Game.Services.Interfaces;
using Game.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Game.Services;

public class DungeonService : IDungeonService
{
    private static readonly Random Generator = Random.Shared;

    private readonly ILogger<DungeonService> _logger;
    private readonly IMapper _mapper;
    private readonly IProofOfWork _proofOfWork;
    private readonly GameDbContext _dbContext;
    private readonly PlayDungeonReplyProducer _playDungeonReplyProducer;

    public DungeonService(
        ILogger<DungeonService> logger,
        IMapper mapper,
        GameDbContext dbContext,
        IProofOfWork proofOfWork,
        PlayDungeonReplyProducer playDungeonReplyProducer
    )
    {
        _logger = logger;
        _mapper = mapper;
        _dbContext = dbContext;
        _proofOfWork = proofOfWork;
        _playDungeonReplyProducer = playDungeonReplyProducer;
    }

    public async Task<IEnumerable<DungeonViewModel>> GetAll()
    {
        var dungeons = await _dbContext.Dungeons
                                       .Include(d => d.Rewards)
                                       .OrderByDescending(d => d.Id)
                                       .ToListAsync();

        return _mapper.Map<IEnumerable<DungeonViewModel>>(dungeons);
    }

    public async Task<Result<DungeonViewModel>> GetByTransactionId(Guid transactionId)
    {
        var dungeon = await _dbContext.Dungeons
                                      .Include(d => d.Rewards)
                                      .FirstOrDefaultAsync(a => a.TransactionId == transactionId);

        return dungeon == null
            ? Result.Fail<DungeonViewModel>($"A dungeon with uuid equal to '{transactionId}' could not be found")
            : Result.Ok(_mapper.Map<DungeonViewModel>(dungeon));
    }

    public async Task ProcessPlayDungeonGameRequest(PlayDungeonGameDto dto)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            await ConsumePlayDungeon(dto);

            LogInformation(dto, "Successfully processed");

            await transaction.CommitAsync();
            return;
        }
        catch (PlayDungeonFinishException ex)
        {
            PublishDungeonErrorToFinish(
                dto: dto,
                errorMessage: ex.Message
            );
        }
        catch (RabbitMqException rex)
        {
            LogCritical(dto, rex);
        }

        await transaction.RollbackAsync();
    }

    private async Task ConsumePlayDungeon(PlayDungeonGameDto dto)
    {
        var dungeonEntranceTransactionId = dto.DungeonEntranceTransactionId;

        var entrance = await _dbContext
                             .DungeonEntrances
                             .Include(de => de.Dungeon.Rewards)
                             .FirstOrDefaultAsync(de => de.TransactionId == dungeonEntranceTransactionId);

        if (entrance == null)
            throw new PlayDungeonFinishException($"Dungeon entrance {dungeonEntranceTransactionId} not found");

        var stopwatch = new Stopwatch();

        stopwatch.Start();

        var hashFound = await _proofOfWork.FindHash(entrance.Dungeon.Difficulty);

        stopwatch.Stop();

        var reward = hashFound
            ? entrance.Dungeon.Rewards.MinBy(_ => Generator.Next(int.MaxValue))
            : default;

        var earnedExperience = hashFound
            ? Generator.Next(entrance.Dungeon.MinExperience, entrance.Dungeon.MaxExperience)
            : default(int?);

        var earnedGold = hashFound
            ? Generator.Next(entrance.Dungeon.MinGold, entrance.Dungeon.MaxGold)
            : default(int?);

        var dungeonJournal = new DungeonJournals
        {
            WasSuccessful = hashFound,
            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
            CharacterTransactionId = entrance.TransactionId,
            DungeonEntranceTransactionId = entrance.TransactionId,
            Dungeon = entrance.Dungeon,
            EarnedGold = earnedGold,
            EarnedExperience = earnedExperience,
            Reward = reward,
        };

        _dbContext.DungeonJournals.Add(dungeonJournal);

        if (await _dbContext.SaveChangesAsync() <= 0)
            throw new PlayDungeonFinishException($"Could not save journal for dungeon entrance {entrance.TransactionId}");

        _dbContext.DungeonEntrances.Remove(entrance);

        if (await _dbContext.SaveChangesAsync() <= 0)
            throw new PlayDungeonFinishException($"Could not remove dungeon entrance {entrance.TransactionId}");

        var itemRewardDto = reward switch
        {
            Armors armor => new ItemRewardDto
            {
                Resistance = Generator.Next(armor.Resistance, armor.Resistance + armor.MaxQuality),
                TransactionId = armor.TransactionId,
                Name = armor.Name,
                Rarity = (int)armor.Rarity,
                Price = armor.Price,
            },
            Weapons weapon => new ItemRewardDto
            {
                Power = Generator.Next(weapon.Power, weapon.Power + weapon.MaxQuality),
                TransactionId = weapon.TransactionId,
                Name = weapon.Name,
                Rarity = (int)weapon.Rarity,
                Price = weapon.Price,
            },
            _ => null,
        };

        var resultString = hashFound ? "WIN" : "LOSS";
        LogInformation(dto, $"Entrance was successfully consumed and the dungeon \"{entrance.Dungeon.Name}\" result is a {resultString}");

        _playDungeonReplyProducer.Publish(
            @event: new PlayDungeonReplyDto
            {
                PlayDungeonEvent = PlayDungeonEventEnum.DungeonFinished,
                DungeonEntranceTransactionId = entrance.TransactionId,
                EarnedItem = itemRewardDto,
                EarnedGold = earnedGold,
                EarnedExperience = earnedExperience,
                SagaName = dto.SagaName,
                SagaCorrelationId = dto.SagaCorrelationId,
            }
        );
    }

    private void PublishDungeonErrorToFinish(PlayDungeonGameDto dto, string errorMessage)
    {
        var @event = new PlayDungeonReplyDto
        {
            PlayDungeonEvent = PlayDungeonEventEnum.DungeonErrorToFinish,
            DungeonEntranceTransactionId = dto.DungeonEntranceTransactionId,
            SagaName = dto.SagaName,
            SagaCorrelationId = dto.SagaCorrelationId,
        };

        LogInformation(dto, $"Sending {@event.PlayDungeonEvent} event to armory reply queue. Error message: {errorMessage}");

        _playDungeonReplyProducer.Publish(@event);
    }

    private void LogInformation(PlayDungeonGameDto dto, string message)
    {
        _logger.LogInformation(
            "[{SagaName} #{SagaCorrelationId}] [DungeonEntrance #{TransactionId}] [{EventName}] {EventMessage}",
            dto.SagaName,
            dto.SagaCorrelationId,
            dto.DungeonEntranceTransactionId,
            dto.PlayDungeonEvent,
            message
        );
    }

    private void LogCritical(PlayDungeonGameDto dto, Exception ex)
    {
        _logger.LogCritical(
            ex,
            "[{SagaName} #{SagaCorrelationId}] [DungeonEntrance #{TransactionId}] [{EventName}] Failed to process. Message: {EventMessage}",
            dto.SagaName,
            dto.SagaCorrelationId,
            dto.DungeonEntranceTransactionId,
            dto.PlayDungeonEvent,
            string.IsNullOrEmpty(ex.Message) ? "Unknown error" : ex.Message
        );
    }
}
