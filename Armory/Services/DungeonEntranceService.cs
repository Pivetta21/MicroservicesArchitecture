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
            "Character with uuid {} requested an entrance registration for dungeon {}",
            character.TransactionId,
            body.DungeonTransactionId
        );

        return Result.Ok("Your dungeon entrance request was sent successfully and will be processed soon");
    }

    public Task ProcessDungeonEntrance(DungeonEntranceGameDto dto)
    {
        switch (dto.DungeonEntranceEvent)
        {
            case DungeonEntranceEventEnum.RollbackCreate:
                return RollbackRegistration(dto.DungeonEntranceTransactionId);
            case DungeonEntranceEventEnum.ChargeFee:
                return ChargeFeeRegistration(dto);
            case DungeonEntranceEventEnum.RollbackChargeFee:
                return RollbackFeeRegistration(dto);
            case DungeonEntranceEventEnum.RegisterEntrance:
            case DungeonEntranceEventEnum.ProcessRegistration:
            case DungeonEntranceEventEnum.ProcessChargeFeeError:
            default:
                return Task.CompletedTask;
        }
    }

    private async Task RollbackFeeRegistration(DungeonEntranceGameDto dto)
    {
        // TODO add payed amount from entrance to character gold and mark as ErrorToProcess
    }

    private async Task RollbackRegistration(Guid dungeonEntranceTransactionId)
    {
        var dungeonEntrance = await _dbContext
                                    .DungeonEntrances
                                    .FirstOrDefaultAsync(x => x.TransactionId == dungeonEntranceTransactionId);

        if (dungeonEntrance == null)
        {
            // TODO probably needs a proper rollback mechanism (and maybe already exist)
            _logger.LogInformation("Dungeon entrance {} not found", dungeonEntranceTransactionId);
            return;
        }

        _dbContext.DungeonEntrances.Remove(dungeonEntrance);
        var writtenEntries = await _dbContext.SaveChangesAsync();

        if (writtenEntries <= 0)
            _logger.LogCritical("Dungeon entrance {} could not be removed", dungeonEntranceTransactionId);
    }

    private async Task ChargeFeeRegistration(DungeonEntranceGameDto dto)
    {
        var dungeonEntrance = await _dbContext
                                    .DungeonEntrances
                                    .Include(x => x.Character)
                                    .FirstOrDefaultAsync(x => x.TransactionId == dto.DungeonEntranceTransactionId);

        if (dungeonEntrance == null)
        {
            _logger.LogWarning("Dungeon entrance with uuid {} not found", dto.DungeonEntranceTransactionId);

            PublishProcessChargeFeeError(dto.DungeonEntranceTransactionId);
            return;
        }

        if (dto.DungeonCost == null || dungeonEntrance.Character.Gold - dto.DungeonCost < 0)
        {
            dungeonEntrance.Status = DungeonEntranceStatusEnum.ErrorOnFeePayment;

            PublishProcessChargeFeeError(dungeonEntrance.DungeonTransactionId);
            return;
        }

        dungeonEntrance.Character.Gold -= dto.DungeonCost.Value;
        dungeonEntrance.PayedFee = dto.DungeonCost.Value;
        dungeonEntrance.Status = DungeonEntranceStatusEnum.ReadyToUse;

        _dbContext.DungeonEntrances.Update(dungeonEntrance);
        var writtenEntries = await _dbContext.SaveChangesAsync();

        if (writtenEntries <= 0)
        {
            _logger.LogWarning("Dungeon entrance {} could not be updated", dungeonEntrance.TransactionId);
            return;
        }

        _dungeonEntranceProducer.Publish(
            @event: new DungeonEntranceArmoryDto
            {
                DungeonEntranceEvent = DungeonEntranceEventEnum.ProcessRegistration,
                DungeonTransactionId = dungeonEntrance.DungeonTransactionId,
                CharacterTransactionId = dungeonEntrance.Character.TransactionId,
                DungeonEntranceTransactionId = dungeonEntrance.TransactionId,
            }
        );

        _logger.LogInformation(
            ""
        );
    }

    private void PublishProcessChargeFeeError(Guid dungeonEntranceTransactionId)
    {
        _logger.LogInformation(
            "Fail to charge fee for dungeon entrance {}, sending {} event to Armory queue",
            dungeonEntranceTransactionId,
            DungeonEntranceEventEnum.ProcessChargeFeeError
        );

        _dungeonEntranceProducer.Publish(
            @event: new DungeonEntranceArmoryDto
            {
                DungeonEntranceEvent = DungeonEntranceEventEnum.ProcessChargeFeeError,
                DungeonEntranceTransactionId = dungeonEntranceTransactionId,
            }
        );
    }
}
