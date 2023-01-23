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
            await ConsumePlayDungeon(dto.DungeonEntranceTransactionId);

            await transaction.CommitAsync();
            return;
        }
        catch (PlayDungeonFinishException ex)
        {
            PublishDungeonErrorToFinish(
                dungeonEntranceTransactionId: dto.DungeonEntranceTransactionId,
                ex.Message
            );
        }
        catch (RabbitMqException rex)
        {
            _logger.LogCritical(
                "Error when processing event {PlayDungeonEvent} for dungeon entrance {DungeonEntranceTransactionId}. Message: {Message}",
                dto.PlayDungeonEvent,
                dto.DungeonEntranceTransactionId,
                rex.Message
            );
        }

        await transaction.RollbackAsync();
    }

    private async Task ConsumePlayDungeon(Guid dungeonEntranceTransactionId)
    {
        var entrance = await _dbContext
                             .DungeonEntrances
                             .Include(de => de.Dungeon.Rewards)
                             .FirstOrDefaultAsync(de => de.TransactionId == dungeonEntranceTransactionId);

        if (entrance == null)
            throw new PlayDungeonFinishException($"Dungeon entrance {dungeonEntranceTransactionId} not found");

        var dungeonResult = await _proofOfWork.FindHash(entrance.Dungeon.Difficulty);

        var reward = dungeonResult.Success
            ? entrance.Dungeon.Rewards.MinBy(_ => Generator.Next(int.MaxValue))
            : null;

        var dungeonJournal = new DungeonJournals
        {
            WasSuccessful = dungeonResult.Success,
            ElapsedMilliseconds = dungeonResult.Time,
            CharacterTransactionId = entrance.TransactionId,
            DungeonEntranceTransactionId = entrance.TransactionId,
            Dungeon = entrance.Dungeon,
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
                Price = armor.Price
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

        _logger.LogInformation(
            "Dungeon entrance {DungeonEntranceTransactionId} consumed successfully, sending {DungeonEntranceEvent} event to armory reply queue",
            entrance.TransactionId,
            PlayDungeonEventEnum.DungeonFinished
        );

        _playDungeonReplyProducer.Publish(
            @event: new PlayDungeonReplyDto
            {
                PlayDungeonEvent = PlayDungeonEventEnum.DungeonFinished,
                DungeonEntranceTransactionId = entrance.TransactionId,
                ItemReward = itemRewardDto,
            }
        );
    }

    private void PublishDungeonErrorToFinish(Guid dungeonEntranceTransactionId, string errorMessage)
    {
        _logger.LogInformation(
            "{}, sending {PlayDungeonEventEnum} event to armory reply queue",
            errorMessage,
            PlayDungeonEventEnum.DungeonErrorToFinish
        );

        _playDungeonReplyProducer.Publish(
            @event: new PlayDungeonReplyDto
            {
                PlayDungeonEvent = PlayDungeonEventEnum.DungeonErrorToFinish,
                DungeonEntranceTransactionId = dungeonEntranceTransactionId,
            }
        );
    }
}
