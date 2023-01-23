using AutoMapper;
using Common.DTOs.DungeonEntrance;
using FluentResults;
using Game.AsyncDataServices;
using Game.Data;
using Game.Models;
using Game.Services.Interfaces;
using Game.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Game.Services;

public class DungeonEntranceService : IDungeonEntranceService
{
    private readonly IMapper _mapper;
    private readonly ILogger<DungeonEntranceService> _logger;
    private readonly GameDbContext _dbContext;

    private readonly DungeonEntranceProducer _dungeonEntranceProducer;

    public DungeonEntranceService(
        IMapper mapper,
        ILogger<DungeonEntranceService> logger,
        GameDbContext dbContext,
        DungeonEntranceProducer dungeonEntranceProducer
    )
    {
        _mapper = mapper;
        _logger = logger;
        _dbContext = dbContext;
        _dungeonEntranceProducer = dungeonEntranceProducer;
    }

    public async Task<IEnumerable<DungeonEntranceViewModel>> GetAll()
    {
        var entrances = await _dbContext.DungeonEntrances
                                        .OrderByDescending(e => e.Id)
                                        .ToListAsync();

        return _mapper.Map<IEnumerable<DungeonEntranceViewModel>>(entrances);
    }

    public async Task<Result<DungeonEntranceViewModel>> GetByTransactionId(Guid transactionId)
    {
        var entrance = await _dbContext.DungeonEntrances
                                       .FirstOrDefaultAsync(x => x.TransactionId == transactionId);

        return entrance == null
            ? Result.Fail($"A dungeon entrance with uuid equal to '{transactionId}' could not be found")
            : Result.Ok(_mapper.Map<DungeonEntranceViewModel>(entrance));
    }

    public async Task ProcessDungeonEntrance(DungeonEntranceArmoryDto dto)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            switch (dto.DungeonEntranceEvent)
            {
                case DungeonEntranceEventEnum.RegisterEntrance:
                    await ConsumeRegisterEntrance(dto);
                    break;
                case DungeonEntranceEventEnum.ProcessEntrance:
                    await ConsumeProcessEntrance(dto);
                    break;
                case DungeonEntranceEventEnum.ProcessEntranceError:
                    await ConsumeProcessEntranceError(dto);
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
        catch (DungeonEntranceFeeException feeEx)
        {
            PublishRollbackChargeFee(
                dungeonEntranceTransactionId: dto.DungeonEntranceTransactionId,
                errorMessage: feeEx.Message
            );
        }
        catch (DungeonEntranceRollbackException rollbackEx)
        {
            PublishRollbackEntrance(
                dungeonEntranceTransactionId: dto.DungeonEntranceTransactionId,
                errorMessage: rollbackEx.Message
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

    private async Task ConsumeRegisterEntrance(DungeonEntranceArmoryDto dto)
    {
        if (dto.CharacterTransactionId == null)
            throw new DungeonEntranceRollbackException($"Field {nameof(dto.CharacterTransactionId)} should not be null");

        var dungeon = await _dbContext.Dungeons.FirstOrDefaultAsync(d => d.TransactionId == dto.DungeonTransactionId);

        if (dungeon == null)
            throw new DungeonEntranceRollbackException($"Dungeon with uuid {dto.DungeonTransactionId} not found");

        var dungeonEntrance = new DungeonEntrances
        {
            CharacterTransactionId = dto.CharacterTransactionId!.Value,
            TransactionId = dto.DungeonEntranceTransactionId,
            Processed = false,
            Dungeon = dungeon,
        };

        _dbContext.DungeonEntrances.Add(dungeonEntrance);

        if (await _dbContext.SaveChangesAsync() <= 0)
            throw new DungeonEntranceRollbackException($"Dungeon entrance {dto.DungeonTransactionId} could not be created");

        _logger.LogInformation(
            "Dungeon entrance {DungeonEntranceTransactionId} persisted successfully, sending {DungeonEntranceEvent} event to Armory queue",
            dungeonEntrance.TransactionId,
            DungeonEntranceEventEnum.ChargeFee
        );

        _dungeonEntranceProducer.Publish(
            @event: new DungeonEntranceGameDto
            {
                DungeonEntranceTransactionId = dungeonEntrance.TransactionId,
                DungeonEntranceEvent = DungeonEntranceEventEnum.ChargeFee,
                DungeonCost = dungeon.Cost,
            }
        );
    }

    private async Task ConsumeProcessEntrance(DungeonEntranceArmoryDto dto)
    {
        var dungeonEntrance = await GetDungeonEntranceByTransactionId(dto.DungeonEntranceTransactionId);

        if (dungeonEntrance == null)
            throw new DungeonEntranceFeeException($"Dungeon with uuid {dto.DungeonTransactionId} not found");

        dungeonEntrance.Processed = true;

        _dbContext.DungeonEntrances.Update(dungeonEntrance);

        if (await _dbContext.SaveChangesAsync() <= 0)
            throw new DungeonEntranceFeeException($"Dungeon entrance {dto.DungeonTransactionId} could not be processed");

        _logger.LogInformation(
            "Dungeon entrance {DungeonEntranceTransactionId} processed successfully",
            dungeonEntrance.TransactionId
        );
    }

    private async Task ConsumeProcessEntranceError(DungeonEntranceArmoryDto dto)
    {
        var entranceGuid = dto.DungeonEntranceTransactionId;
        var dungeonEntrance = await GetDungeonEntranceByTransactionId(entranceGuid);

        if (dungeonEntrance == null)
            throw new DungeonEntranceRollbackException($"Dungeon entrance not found {entranceGuid}");

        _dbContext.DungeonEntrances.Remove(dungeonEntrance);

        if (await _dbContext.SaveChangesAsync() <= 0)
            throw new RabbitMqException($"Dungeon entrance {dungeonEntrance.TransactionId} could not be removed");

        PublishRollbackEntrance(
            dungeonEntranceTransactionId: dungeonEntrance.TransactionId,
            errorMessage: $"Dungeon entrance {dungeonEntrance.TransactionId} was successfully removed"
        );
    }

    private Task<DungeonEntrances?> GetDungeonEntranceByTransactionId(Guid transactionId)
    {
        return _dbContext
               .DungeonEntrances
               .FirstOrDefaultAsync(x => x.TransactionId == transactionId);
    }

    private void PublishRollbackEntrance(Guid dungeonEntranceTransactionId, string errorMessage)
    {
        _logger.LogInformation(
            "{}, sending {DungeonEntranceEvent} event to Armory queue",
            errorMessage,
            DungeonEntranceEventEnum.RollbackEntrance
        );

        _dungeonEntranceProducer.Publish(
            @event: new DungeonEntranceGameDto
            {
                DungeonEntranceTransactionId = dungeonEntranceTransactionId,
                DungeonEntranceEvent = DungeonEntranceEventEnum.RollbackEntrance,
            }
        );
    }

    private void PublishRollbackChargeFee(Guid dungeonEntranceTransactionId, string errorMessage)
    {
        _logger.LogInformation(
            "{}, sending {DungeonEntranceEvent} to Armory queue",
            errorMessage,
            DungeonEntranceEventEnum.RollbackChargeFee
        );

        _dungeonEntranceProducer.Publish(
            @event: new DungeonEntranceGameDto
            {
                DungeonEntranceTransactionId = dungeonEntranceTransactionId,
                DungeonEntranceEvent = DungeonEntranceEventEnum.RollbackChargeFee,
            }
        );
    }
}
