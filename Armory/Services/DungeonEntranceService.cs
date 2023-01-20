using Armory.AsyncDataServices;
using Armory.Data;
using Armory.Models.Enums;
using Armory.Models;
using Armory.Services.Interfaces;
using Armory.ViewModels;
using Common.DTOs.DungeonEntrance;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Armory.Services;

public class DungeonEntranceService : IDungeonEntranceService
{
    private readonly ILogger<DungeonEntranceService> _logger;
    private readonly ArmoryDbContext _dbContext;
    private readonly DungeonEntranceProducer _dungeonEntranceProducer;

    public DungeonEntranceService(
        ILogger<DungeonEntranceService> logger,
        ArmoryDbContext dbContext,
        DungeonEntranceProducer dungeonEntranceProducer
    )
    {
        _logger = logger;
        _dbContext = dbContext;
        _dungeonEntranceProducer = dungeonEntranceProducer;
    }

    public async Task<Result<string>> RegisterEntrance(DungeonRegisterEntranceViewModel body)
    {
        var character = await _dbContext
                              .Characters
                              .FirstOrDefaultAsync(x => x.TransactionId == body.CharacterTransactionId);

        if (character == null)
            return Result.Fail($"Character with uuid '{body.CharacterTransactionId}' not found");

        var entrance = new DungeonEntrances
        {
            Character = character,
            DungeonTransactionId = body.DungeonTransactionId,
            TransactionId = Guid.NewGuid(),
            Status = DungeonEntranceStatusEnum.RegistrationRequested,
            Deleted = false,
        };

        _dbContext.DungeonEntrances.Add(entrance);
        var writtenEntries = await _dbContext.SaveChangesAsync();

        if (writtenEntries <= 0)
            return Result.Fail("Entrance could not be persisted");

        _dungeonEntranceProducer.Publish(
            @event: new DungeonEntranceArmoryDto
            {
                DungeonEntranceEvent = DungeonEntranceEventEnum.RegisterEntrance,
                DungeonTransactionId = body.DungeonTransactionId,
                DungeonEntranceTransactionId = entrance.TransactionId,
                CharacterTransactionId = entrance.Character.TransactionId,
            }
        );

        _logger.LogInformation(
            "Character with uuid {CharacterTransactionId} requested an entrance registration for dungeon {DungeonTransactionId}",
            character.TransactionId,
            body.DungeonTransactionId
        );

        return Result.Ok("Your dungeon entrance request was sent successfully and will be processed soon");
    }

    public Task ProcessDungeonEntrance(DungeonEntranceGameDto dto)
    {
        return dto.DungeonEntranceEvent switch
        {
            DungeonEntranceEventEnum.ChargeFee => ConsumeChargeFee(dto),
            DungeonEntranceEventEnum.RollbackChargeFee => ConsumeRollbackChargeFee(dto),
            DungeonEntranceEventEnum.RollbackEntrance => ConsumeRollbackEntrance(dto.DungeonEntranceTransactionId),
            _ => Task.CompletedTask,
        };
    }

    private async Task ConsumeChargeFee(DungeonEntranceGameDto dto)
    {
        var dungeonEntrance = await GetDungeonEntranceByTransactionId(dto.DungeonEntranceTransactionId);

        if (dungeonEntrance == null || dto.DungeonCost == null)
        {
            PublishProcessEntranceError(
                dungeonEntranceTransactionId: dto.DungeonEntranceTransactionId,
                errorMessage: dto.DungeonCost == null
                    ? $"Field {nameof(dto.DungeonCost)} should not be null"
                    : $"Dungeon entrance with uuid {dto.DungeonEntranceTransactionId} not found"
            );

            return;
        }

        if (dungeonEntrance.Character.Gold - dto.DungeonCost < 0)
        {
            PublishProcessEntranceError(
                dungeonEntranceTransactionId: dungeonEntrance.TransactionId,
                errorMessage: $"Character {dungeonEntrance.Character.TransactionId} does not have enough gold"
            );

            return;
        }

        dungeonEntrance.Character.Gold -= dto.DungeonCost.Value;
        dungeonEntrance.PayedFee = dto.DungeonCost.Value;
        dungeonEntrance.Status = DungeonEntranceStatusEnum.ReadyToUse;

        _dbContext.DungeonEntrances.Update(dungeonEntrance);
        var writtenEntries = await _dbContext.SaveChangesAsync();

        if (writtenEntries <= 0)
        {
            PublishProcessEntranceError(
                dungeonEntranceTransactionId: dungeonEntrance.TransactionId,
                errorMessage: $"Dungeon entrance {dungeonEntrance.TransactionId} could not be updated"
            );

            return;
        }

        _logger.LogInformation(
            "Fee charged successfully for dungeon entrance {DungeonEntranceTransactionId}, sending {DungeonEntranceEvent} event to Game queue",
            dungeonEntrance.TransactionId,
            DungeonEntranceEventEnum.ProcessEntrance
        );

        _dungeonEntranceProducer.Publish(
            @event: new DungeonEntranceArmoryDto
            {
                DungeonEntranceEvent = DungeonEntranceEventEnum.ProcessEntrance,
                DungeonTransactionId = dungeonEntrance.DungeonTransactionId,
                CharacterTransactionId = dungeonEntrance.Character.TransactionId,
                DungeonEntranceTransactionId = dungeonEntrance.TransactionId,
            }
        );
    }

    private async Task ConsumeRollbackChargeFee(DungeonEntranceGameDto dto)
    {
        var dungeonEntrance = await GetDungeonEntranceByTransactionId(dto.DungeonEntranceTransactionId);

        if (dungeonEntrance?.PayedFee == null)
        {
            PublishProcessEntranceError(
                dungeonEntranceTransactionId: dto.DungeonEntranceTransactionId,
                errorMessage: dungeonEntrance == null
                    ? $"Dungeon entrance not found {dto.DungeonEntranceTransactionId}"
                    : $"Field {nameof(dungeonEntrance.PayedFee)} should not be null"
            );

            return;
        }

        // Ideally should have a table to track changes that happened to an entity,
        // and then retrieve the PayedFee from there
        dungeonEntrance.Character.Gold += dungeonEntrance.PayedFee.Value;

        _dbContext.Characters.Update(dungeonEntrance.Character);
        var writtenEntries = await _dbContext.SaveChangesAsync();

        if (writtenEntries <= 0)
        {
            PublishProcessEntranceError(
                dungeonEntranceTransactionId: dungeonEntrance.TransactionId,
                errorMessage: $"Character with uuid {dungeonEntrance.Character.TransactionId} could not be refunded"
            );
        }
        else
        {
            _logger.LogInformation(
                "Character with uuid {CharacterTransactionId} was refunded successfully",
                dungeonEntrance.Character.TransactionId
            );
        }
    }

    private async Task ConsumeRollbackEntrance(Guid dungeonEntranceTransactionId)
    {
        var dungeonEntrance = await GetDungeonEntranceByTransactionId(dungeonEntranceTransactionId);

        if (dungeonEntrance == null)
            throw new Exception($"Dungeon entrance {dungeonEntranceTransactionId} not found");

        dungeonEntrance.Status = DungeonEntranceStatusEnum.RegistrationFailed;
        dungeonEntrance.Deleted = true;

        _dbContext.DungeonEntrances.Update(dungeonEntrance);
        var writtenEntries = await _dbContext.SaveChangesAsync();

        if (writtenEntries <= 0)
        {
            _logger.LogCritical(
                "Dungeon entrance {DungeonEntranceTransactionId} could not be deleted",
                dungeonEntranceTransactionId
            );

            return;
        }

        _logger.LogInformation(
            "Dungeon entrance {DungeonEntranceTransactionId} was successfully deleted",
            dungeonEntranceTransactionId
        );
    }

    private Task<DungeonEntrances?> GetDungeonEntranceByTransactionId(Guid transactionId)
    {
        return _dbContext
               .DungeonEntrances
               .Include(x => x.Character)
               .FirstOrDefaultAsync(x => x.TransactionId == transactionId);
    }

    private void PublishProcessEntranceError(Guid dungeonEntranceTransactionId, string errorMessage)
    {
        _logger.LogInformation(
            "{}, sending {DungeonEntranceEvent} event to Game queue",
            errorMessage,
            DungeonEntranceEventEnum.ProcessEntranceError
        );

        _dungeonEntranceProducer.Publish(
            @event: new DungeonEntranceArmoryDto
            {
                DungeonEntranceEvent = DungeonEntranceEventEnum.ProcessEntranceError,
                DungeonEntranceTransactionId = dungeonEntranceTransactionId,
            }
        );
    }
}
