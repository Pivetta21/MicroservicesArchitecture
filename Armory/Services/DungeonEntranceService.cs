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

    public async Task<Result<string>> RegisterEntrance(DungeonRegisterEntranceViewModel body, Guid dungeonTransactionId)
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
            TransactionId = dungeonTransactionId,
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

    public async Task ProcessDungeonEntrance(DungeonEntranceGameDto dto)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            switch (dto.DungeonEntranceEvent)
            {
                case DungeonEntranceEventEnum.ChargeFee:
                    await ConsumeChargeFee(dto);
                    break;
                case DungeonEntranceEventEnum.RollbackChargeFee:
                    await ConsumeRollbackChargeFee(dto);
                    break;
                case DungeonEntranceEventEnum.RollbackEntrance:
                    await ConsumeRollbackEntrance(dto.DungeonEntranceTransactionId);
                    break;
            }

            _logger.LogInformation(
                "Event {DungeonEntranceEvent} for dungeon entrance {DungeonEntranceTransactionId} was successfully processed",
                dto.DungeonEntranceEvent,
                dto.DungeonEntranceTransactionId
            );

            await transaction.CommitAsync();
            return;
        }
        catch (DungeonEntranceErrorException errorEx)
        {
            PublishProcessEntranceError(
                dungeonEntranceTransactionId: dto.DungeonEntranceTransactionId,
                errorMessage: errorEx.Message
            );
        }
        catch (RabbitMqException rex)
        {
            _logger.LogCritical(
                "Error when processing event {DungeonEntranceEvent} for dungeon entrance {DungeonEntranceTransactionId}. Message: {Message}",
                dto.DungeonEntranceEvent,
                dto.DungeonEntranceTransactionId,
                rex.Message
            );
        }

        await transaction.RollbackAsync();
    }

    private async Task ConsumeChargeFee(DungeonEntranceGameDto dto)
    {
        if (dto.DungeonCost == null)
            throw new DungeonEntranceErrorException($"Field {nameof(dto.DungeonCost)} should not be null");

        var entranceGuid = dto.DungeonEntranceTransactionId;

        var dungeonEntrance = await GetDungeonEntranceByTransactionId(entranceGuid);

        if (dungeonEntrance == null)
            throw new DungeonEntranceErrorException($"Dungeon entrance {entranceGuid} not found");

        if (dungeonEntrance.Character.Gold - dto.DungeonCost < 0)
            throw new DungeonEntranceErrorException($"Not enough gold for dungeon entrance {entranceGuid}");

        dungeonEntrance.Character.Gold -= dto.DungeonCost.Value;
        dungeonEntrance.PayedFee = dto.DungeonCost.Value;
        dungeonEntrance.Status = DungeonEntranceStatusEnum.ReadyToUse;

        _dbContext.DungeonEntrances.Update(dungeonEntrance);

        if (await _dbContext.SaveChangesAsync() <= 0)
            throw new DungeonEntranceErrorException($"Dungeon entrance {entranceGuid} could not be updated");

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
        var entranceGuid = dto.DungeonEntranceTransactionId;
        var entrance = await GetDungeonEntranceByTransactionId(entranceGuid);

        if (entrance == null)
            throw new DungeonEntranceErrorException($"Dungeon entrance {entranceGuid} not found");

        if (entrance.PayedFee == null)
            throw new DungeonEntranceErrorException($"Dungeon entrance {entranceGuid} must have a payed fee");

        entrance.Character.Gold += entrance.PayedFee.Value;
        _dbContext.Characters.Update(entrance.Character);

        if (await _dbContext.SaveChangesAsync() <= 0)
            throw new DungeonEntranceErrorException($"Character {entrance.Character.TransactionId} couldn't be refunded");

        _logger.LogInformation(
            "Character with uuid {CharacterTransactionId} was refunded successfully",
            entrance.Character.TransactionId
        );
    }

    private async Task ConsumeRollbackEntrance(Guid dungeonEntranceTransactionId)
    {
        var dungeonEntrance = await GetDungeonEntranceByTransactionId(dungeonEntranceTransactionId);

        if (dungeonEntrance == null)
            throw new Exception($"Dungeon entrance {dungeonEntranceTransactionId} not found");

        dungeonEntrance.Status = DungeonEntranceStatusEnum.RegistrationFailed;
        dungeonEntrance.Deleted = true;

        _dbContext.DungeonEntrances.Update(dungeonEntrance);

        if (await _dbContext.SaveChangesAsync() <= 0)
            throw new RabbitMqException($"Dungeon entrance {dungeonEntranceTransactionId} could not be deleted");

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
