using Armory.AsyncDataServices;
using Armory.Data;
using Armory.Models;
using Armory.Models.Enums;
using Armory.Services.Interfaces;
using Armory.ViewModels;
using AutoMapper;
using Common.DTOs.PlayDungeon;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Armory.Services;

public class DungeonService : IDungeonService
{
    private readonly IMapper _mapper;
    private readonly ILogger<DungeonService> _logger;
    private readonly ArmoryDbContext _dbContext;
    private readonly PlayDungeonGameRequestProducer _playDungeonGameRequestProducer;
    private readonly PlayDungeonReplyProducer _playDungeonReplyProducer;

    public DungeonService(
        IMapper mapper,
        ILogger<DungeonService> logger,
        ArmoryDbContext dbContext,
        PlayDungeonGameRequestProducer playDungeonGameRequestProducer,
        PlayDungeonReplyProducer playDungeonReplyProducer
    )
    {
        _mapper = mapper;
        _dbContext = dbContext;
        _playDungeonGameRequestProducer = playDungeonGameRequestProducer;
        _playDungeonReplyProducer = playDungeonReplyProducer;
        _logger = logger;
    }

    public async Task<Result<DungeonEntranceViewModel>> PlayDungeon(PlayDungeonViewModel playDungeonViewModel)
    {
        var dungeonEntranceTransactionId = playDungeonViewModel.DungeonEntranceTransactionId;

        var entrance = await _dbContext
                             .DungeonEntrances
                             .FirstOrDefaultAsync(de => de.TransactionId == dungeonEntranceTransactionId);

        if (entrance == null)
            return Result.Fail<DungeonEntranceViewModel>($"Dungeon entrance {dungeonEntranceTransactionId} not found");

        entrance.Status = DungeonEntranceStatusEnum.AwaitingProcessing;
        _dbContext.DungeonEntrances.Update(entrance);

        if (await _dbContext.SaveChangesAsync() <= 0)
            return Result.Fail<DungeonEntranceViewModel>($"Dungeon entrance {entrance.TransactionId} could not be updated");

        _playDungeonGameRequestProducer.Publish(
            @event: new PlayDungeonGameDto
            {
                PlayDungeonEvent = PlayDungeonEventEnum.PlayDungeon,
                DungeonEntranceTransactionId = entrance.TransactionId,
            }
        );

        return Result.Ok(_mapper.Map<DungeonEntranceViewModel>(entrance));
    }

    public async Task ProcessPlayDungeonReply(PlayDungeonReplyDto dto)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            switch (dto.PlayDungeonEvent)
            {
                case PlayDungeonEventEnum.DungeonFinished:
                    await ConsumeDungeonFinished(dto);
                    break;
                case PlayDungeonEventEnum.DungeonErrorToFinish:
                    await ConsumeDungeonErrorToFinish(dto);
                    break;
            }

            _logger.LogInformation(
                "Event {PlayDungeonEvent} for dungeon entrance {DungeonEntranceTransactionId} was successfully processed",
                dto.PlayDungeonEvent,
                dto.DungeonEntranceTransactionId
            );

            await transaction.CommitAsync();
            return;
        }
        catch (PlayDungeonFinishException finishEx)
        {
            PublishDungeonErrorToFinish(
                dungeonEntranceTransactionId: dto.DungeonEntranceTransactionId,
                finishEx.Message
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

    private async Task ConsumeDungeonFinished(PlayDungeonReplyDto dto)
    {
        var entranceGuid = dto.DungeonEntranceTransactionId;
        var entrance = await _dbContext.DungeonEntrances
                                       .Include(de => de.Character.Inventory.Items)
                                       .FirstOrDefaultAsync(de => de.TransactionId == entranceGuid);

        if (entrance == null)
            throw new PlayDungeonFinishException($"Dungeon entrance {entranceGuid} not found");

        if (dto.EarnedItem == null || dto.EarnedGold == null || dto.EarnedExperience == null)
        {
            entrance.Status = DungeonEntranceStatusEnum.Processed;
            _dbContext.DungeonEntrances.Update(entrance);

            if (await _dbContext.SaveChangesAsync() <= 0)
                throw new PlayDungeonFinishException($"Could not update dungeon entrance {entrance.TransactionId} status to processed");

            _logger.LogInformation(
                "Dungeon entrance {DungeonEntranceTransactionId} was successfully played, but the dungeon failed and thus no reward earned",
                entrance.TransactionId
            );

            return;
        }

        var reward = dto.EarnedItem switch
        {
            { Power: > 0 } => new Weapons
            {
                Name = dto.EarnedItem.Name,
                Power = dto.EarnedItem.Power.Value,
                Rarity = (RarityEnum)dto.EarnedItem.Rarity,
                TransactionId = dto.EarnedItem.TransactionId,
            },
            { Resistance: > 0 } => new Armors
            {
                Name = dto.EarnedItem.Name,
                Resistance = dto.EarnedItem.Resistance.Value,
                Rarity = (RarityEnum)dto.EarnedItem.Rarity,
                TransactionId = dto.EarnedItem.TransactionId,
            },
            _ => default(Items?),
        };

        if (reward == null)
            throw new PlayDungeonFinishException($"Error to process reward for dungeon entrance {entrance.TransactionId}");

        entrance.Character.Inventory.Items.Add(reward);
        _dbContext.DungeonEntrances.Update(entrance);

        if (await _dbContext.SaveChangesAsync() <= 0)
            throw new PlayDungeonFinishException($"Could not add item {reward.Id} to character inventory");

        entrance.Character.Gold += dto.EarnedGold.Value;
        _dbContext.DungeonEntrances.Update(entrance);

        if (await _dbContext.SaveChangesAsync() <= 0)
            throw new PlayDungeonFinishException($"Could not add {dto.EarnedGold} gold to character");

        entrance.Character.Experience += dto.EarnedExperience.Value;
        entrance.Character.Level = entrance.Character.Experience switch
        {
            >= 0 and < 10 => 1,
            >= 10 and < 50 => 2,
            >= 50 and < 150 => 3,
            >= 150 and < 250 => 4,
            >= 250 and < 750 => 5,
            >= 750 and < 1750 => 6,
            >= 1750 and < 3250 => 7,
            >= 3250 and < 6000 => 8,
            >= 6000 and < 12000 => 9,
            >= 12000 => 10,
            _ => 1,
        };
        _dbContext.DungeonEntrances.Update(entrance);

        if (await _dbContext.SaveChangesAsync() <= 0)
            throw new PlayDungeonFinishException($"Could not add {dto.EarnedExperience} experience to character");

        entrance.Status = DungeonEntranceStatusEnum.Processed;
        _dbContext.DungeonEntrances.Update(entrance);

        if (await _dbContext.SaveChangesAsync() <= 0)
            throw new PlayDungeonFinishException($"Could not update dungeon entrance {entrance.TransactionId} status to processed");

        _logger.LogInformation(
            "Dungeon entrance {DungeonEntranceTransactionId} was successfully played, dungeon completed and thus a reward has been awarded",
            entrance.TransactionId
        );
    }

    private async Task ConsumeDungeonErrorToFinish(PlayDungeonReplyDto dto)
    {
        var entrance = await _dbContext.DungeonEntrances
                                       .Include(de => de.Character)
                                       .FirstOrDefaultAsync(de => de.TransactionId == dto.DungeonEntranceTransactionId);

        if (entrance == null)
            throw new RabbitMqException($"Dungeon entrance {dto.DungeonEntranceTransactionId} not found");

        if (entrance.PayedFee == null)
            throw new RabbitMqException($"Dungeon entrance {dto.DungeonEntranceTransactionId} must have a payed fee");

        entrance.Character.Gold += entrance.PayedFee.Value;
        _dbContext.Characters.Update(entrance.Character);

        if (await _dbContext.SaveChangesAsync() <= 0)
            throw new RabbitMqException($"Character {entrance.Character.TransactionId} could not be refunded");

        entrance.Status = DungeonEntranceStatusEnum.ProcessedWithError;
        _dbContext.DungeonEntrances.Update(entrance);

        if (await _dbContext.SaveChangesAsync() <= 0)
            throw new RabbitMqException($"Something went wrong when update status to {DungeonEntranceStatusEnum.ProcessedWithError}");
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
