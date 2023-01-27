using AutoMapper;
using Common.DTOs.DungeonEntrance;
using FluentResults;
using Game.AsyncDataServices;
using Game.Data;
using Game.Models;
using Game.Services.Interfaces;
using Game.ViewModels;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

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

    public async Task ProcessDungeonEntrance(DungeonEntranceArmoryDto dto, SagaInfo sagaInfo, IBasicProperties props)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            switch (dto.DungeonEntranceEvent)
            {
                case DungeonEntranceEventEnum.RegisterEntrance:
                    await ConsumeRegisterEntrance(dto, sagaInfo, props);
                    break;
                case DungeonEntranceEventEnum.ProcessEntrance:
                    await ConsumeProcessEntrance(dto, sagaInfo);
                    break;
                case DungeonEntranceEventEnum.ProcessEntranceError:
                    await ConsumeProcessEntranceError(dto, sagaInfo);
                    break;
            }

            LogInformation(sagaInfo, dto, "Successfully processed");

            await transaction.CommitAsync();
            return;
        }
        catch (DungeonEntranceFeeException feeEx)
        {
            PublishRollbackChargeFee(
                sagaInfo: sagaInfo,
                dto: dto,
                errorMessage: feeEx.Message
            );
        }
        catch (DungeonEntranceRollbackException rollbackEx)
        {
            PublishRollbackEntrance(
                sagaInfo: sagaInfo,
                dto: dto,
                errorMessage: rollbackEx.Message
            );
        }
        catch (RabbitMqException rex)
        {
            LogCritical(sagaInfo, dto, rex);
        }

        await transaction.RollbackAsync();
    }

    private async Task ConsumeRegisterEntrance(DungeonEntranceArmoryDto dto, SagaInfo sagaInfo, IBasicProperties props)
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
            throw new DungeonEntranceRollbackException($"Entrance could not be created");

        LogInformation(sagaInfo, dto, "Dungeon entrance persisted successfully");

        var @event = new DungeonEntranceGameDto
        {
            DungeonEntranceTransactionId = dungeonEntrance.TransactionId,
            DungeonEntranceEvent = DungeonEntranceEventEnum.ChargeFee,
            DungeonCost = dungeon.Cost,
        };

        _dungeonEntranceProducer.Publish(@event, props);
    }

    private async Task ConsumeProcessEntrance(DungeonEntranceArmoryDto dto, SagaInfo sagaInfo)
    {
        var dungeonEntrance = await GetDungeonEntranceByTransactionId(dto.DungeonEntranceTransactionId);

        if (dungeonEntrance == null)
            throw new DungeonEntranceFeeException($"Dungeon with uuid {dto.DungeonTransactionId} not found");

        dungeonEntrance.Processed = true;
        _dbContext.DungeonEntrances.Update(dungeonEntrance);

        if (await _dbContext.SaveChangesAsync() <= 0)
            throw new DungeonEntranceFeeException("Entrance could not be marked as processed");

        LogInformation(sagaInfo, dto, "Entrance was successfully marked as processed");
    }

    private async Task ConsumeProcessEntranceError(DungeonEntranceArmoryDto dto, SagaInfo sagaInfo)
    {
        var dungeonEntrance = await GetDungeonEntranceByTransactionId(dto.DungeonEntranceTransactionId);

        if (dungeonEntrance == null)
            throw new DungeonEntranceRollbackException("Entrance not found");

        _dbContext.DungeonEntrances.Remove(dungeonEntrance);

        if (await _dbContext.SaveChangesAsync() <= 0)
            throw new RabbitMqException("Entrance could not be removed");

        PublishRollbackEntrance(
            sagaInfo: sagaInfo,
            dto: dto,
            errorMessage: "Entrance was successfully removed"
        );
    }

    private Task<DungeonEntrances?> GetDungeonEntranceByTransactionId(Guid transactionId)
    {
        return _dbContext
               .DungeonEntrances
               .FirstOrDefaultAsync(x => x.TransactionId == transactionId);
    }

    private void PublishRollbackEntrance(SagaInfo sagaInfo, DungeonEntranceArmoryDto dto, string errorMessage)
    {
        var @event = new DungeonEntranceGameDto
        {
            DungeonEntranceTransactionId = dto.DungeonEntranceTransactionId,
            DungeonEntranceEvent = DungeonEntranceEventEnum.RollbackEntrance,
        };

        LogInformation(sagaInfo, dto, $"Sending {@event.DungeonEntranceEvent} event to armory queue. Error message: {errorMessage}");

        _dungeonEntranceProducer.Publish(@event, sagaInfo);
    }

    private void PublishRollbackChargeFee(SagaInfo sagaInfo, DungeonEntranceArmoryDto dto, string errorMessage)
    {
        var @event = new DungeonEntranceGameDto
        {
            DungeonEntranceTransactionId = dto.DungeonEntranceTransactionId,
            DungeonEntranceEvent = DungeonEntranceEventEnum.RollbackChargeFee,
        };

        LogInformation(sagaInfo, dto, $"Sending {@event.DungeonEntranceEvent} event to armory queue. Error message: {errorMessage}");

        _dungeonEntranceProducer.Publish(@event, sagaInfo);
    }

    private void LogInformation(SagaInfo sagaInfo, DungeonEntranceArmoryDto dto, string message)
    {
        _logger.LogInformation(
            "[{SagaName} #{SagaCorrelationId}] [DungeonEntrance #{TransactionId}] [{EventName}] {EventMessage}",
            sagaInfo.SagaName,
            sagaInfo.SagaCorrelationId,
            dto.DungeonEntranceTransactionId,
            dto.DungeonEntranceEvent,
            message
        );
    }

    private void LogCritical(SagaInfo sagaInfo, DungeonEntranceArmoryDto dto, Exception ex)
    {
        _logger.LogCritical(
            ex,
            "[{SagaName} #{SagaCorrelationId}] [DungeonEntrance #{TransactionId}] [{EventName}] Failed to process. Message: {EventMessage}",
            sagaInfo.SagaName,
            sagaInfo.SagaCorrelationId,
            dto.DungeonEntranceTransactionId,
            dto.DungeonEntranceEvent,
            string.IsNullOrEmpty(ex.Message) ? "Unknown error" : ex.Message
        );
    }
}
