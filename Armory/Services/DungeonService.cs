using Armory.AsyncDataServices;
using Armory.Data;
using Armory.Models;
using Armory.Models.Enums;
using Armory.Services.Interfaces;
using Armory.ViewModels;
using Common.DTOs.PlayDungeon;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Armory.Services;

public class DungeonService : IDungeonService
{
    private readonly ILogger<DungeonService> _logger;
    private readonly ArmoryDbContext _dbContext;
    private readonly PlayDungeonGameRequestProducer _playDungeonGameRequestProducer;
    private readonly PlayDungeonReplyProducer _playDungeonReplyProducer;

    public DungeonService(
        ILogger<DungeonService> logger,
        ArmoryDbContext dbContext,
        PlayDungeonGameRequestProducer playDungeonGameRequestProducer,
        PlayDungeonReplyProducer playDungeonReplyProducer
    )
    {
        _dbContext = dbContext;
        _playDungeonGameRequestProducer = playDungeonGameRequestProducer;
        _playDungeonReplyProducer = playDungeonReplyProducer;
        _logger = logger;
    }

    public async Task<Result<string>> PlayDungeon(PlayDungeonViewModel playDungeonViewModel)
    {
        var dungeonEntranceTransactionId = playDungeonViewModel.DungeonEntranceTransactionId;

        var entrance = await _dbContext
                             .DungeonEntrances
                             .FirstOrDefaultAsync(de => de.TransactionId == dungeonEntranceTransactionId);

        if (entrance == null)
            return Result.Fail<string>($"Dungeon entrance {dungeonEntranceTransactionId} not found");

        entrance.Status = DungeonEntranceStatusEnum.AwaitingProcessing;
        _dbContext.DungeonEntrances.Update(entrance);

        if (await _dbContext.SaveChangesAsync() <= 0)
            return Result.Fail<string>($"Dungeon entrance {entrance.TransactionId} could not be updated");

        _playDungeonGameRequestProducer.Publish(
            @event: new PlayDungeonGameDto
            {
                PlayDungeonEvent = PlayDungeonEventEnum.PlayDungeon,
                DungeonEntranceTransactionId = entrance.TransactionId,
            }
        );

        return Result.Ok("Your play dungeon request was sent successfully and will be processed soon");
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

        if (dto.ItemReward == null)
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

        var inventory = entrance.Character.Inventory;

        Items? reward;
        Items? itemToRemove;

        switch (dto.ItemReward)
        {
            case { Power: > 0 }:
                reward = new Weapons
                {
                    Name = dto.ItemReward.Name,
                    Power = dto.ItemReward.Power.Value,
                    Rarity = (RarityEnum)dto.ItemReward.Rarity,
                    TransactionId = dto.ItemReward.TransactionId,
                };

                itemToRemove = inventory.Items.OfType<Weapons>().FirstOrDefault(i => i.Power < dto.ItemReward.Power);

                break;
            case { Resistance: > 0 }:
                reward = new Armors
                {
                    Name = dto.ItemReward.Name,
                    Resistance = dto.ItemReward.Resistance.Value,
                    Rarity = (RarityEnum)dto.ItemReward.Rarity,
                    TransactionId = dto.ItemReward.TransactionId,
                };

                itemToRemove = inventory.Items.OfType<Armors>().FirstOrDefault(i => i.Resistance < dto.ItemReward.Resistance);
                break;
            default:
                itemToRemove = null;
                reward = null;
                break;
        }

        if (reward == null)
            throw new PlayDungeonFinishException($"Error to process the reward for dungeon entrance {entrance.TransactionId}");

        entrance.Character.Gold += dto.ItemReward.Price;
        _dbContext.Characters.Update(entrance.Character);

        if (await _dbContext.SaveChangesAsync() <= 0)
            throw new PlayDungeonFinishException($"Could not update character {entrance.Character.TransactionId} gold");

        if (itemToRemove != null)
        {
            inventory.Items.Remove(itemToRemove);
            _dbContext.Items.Remove(itemToRemove);

            if (await _dbContext.SaveChangesAsync() <= 0)
                throw new PlayDungeonFinishException($"Could not remove item {itemToRemove.Id} from character inventory");
        }

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
            throw new Exception($"Dungeon entrance {dto.DungeonEntranceTransactionId} not found");

        if (entrance.PayedFee == null)
            throw new Exception($"Dungeon entrance {dto.DungeonEntranceTransactionId} must have a payed fee");

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
