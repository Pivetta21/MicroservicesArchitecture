using Armory.AsyncDataServices;
using Armory.Data;
using Armory.Models.Enums;
using Armory.Models;
using Armory.Services.Interfaces;
using Armory.ViewModels;
using AutoMapper;
using Common.DTOs.DungeonEntrance;
using Common.RabbitMq.Enums;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace Armory.Services;

public class DungeonEntranceService : IDungeonEntranceService
{
    private readonly IMapper _mapper;
    private readonly ILogger<DungeonEntranceService> _logger;
    private readonly ArmoryDbContext _dbContext;
    private readonly DungeonEntranceProducer _dungeonEntranceProducer;

    public DungeonEntranceService(
        IMapper mapper,
        ILogger<DungeonEntranceService> logger,
        ArmoryDbContext dbContext,
        DungeonEntranceProducer dungeonEntranceProducer
    )
    {
        _logger = logger;
        _dbContext = dbContext;
        _dungeonEntranceProducer = dungeonEntranceProducer;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DungeonEntranceViewModel>> Get(long? characterId, DungeonEntranceStatusEnum? status)
    {
        var entranceQuery = _dbContext.DungeonEntrances.AsQueryable();

        if (characterId != null)
            entranceQuery = entranceQuery.Where(de => de.CharacterId == characterId);

        if (status != null)
            entranceQuery = entranceQuery.Where(de => de.Status == status);

        var entrances = await entranceQuery.OrderByDescending(c => c.Id).ToListAsync();
        return _mapper.Map<IEnumerable<DungeonEntranceViewModel>>(entrances);
    }

    public async Task<Result<DungeonEntranceViewModel>> RegisterEntrance(DungeonRegisterEntranceViewModel body)
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

        // Starts dungeon entrance saga
        var sagaInfo = new SagaInfo
        {
            SagaName = SagasEnum.DungeonEntranceChoreography.ToString(),
            SagaCorrelationId = Guid.NewGuid().ToString(),
        };

        var @event = new DungeonEntranceArmoryDto
        {
            DungeonEntranceEvent = DungeonEntranceEventEnum.RegisterEntrance,
            DungeonTransactionId = entrance.DungeonTransactionId,
            DungeonEntranceTransactionId = entrance.TransactionId,
            CharacterTransactionId = entrance.Character.TransactionId,
        };

        _dungeonEntranceProducer.Publish(@event, sagaInfo);



        return Result.Ok(_mapper.Map<DungeonEntranceViewModel>(entrance));
    }

    public async Task ProcessDungeonEntrance(DungeonEntranceGameDto dto, SagaInfo sagaInfo, IBasicProperties props)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            switch (dto.DungeonEntranceEvent)
            {
                case DungeonEntranceEventEnum.ChargeFee:
                    await ConsumeChargeFee(dto, sagaInfo, props);
                    break;
                case DungeonEntranceEventEnum.RollbackChargeFee:
                    await ConsumeRollbackChargeFee(dto, sagaInfo);
                    break;
                case DungeonEntranceEventEnum.RollbackEntrance:
                    await ConsumeRollbackEntrance(dto, sagaInfo);
                    break;
            }

            LogInformation(sagaInfo, dto, "Successfully processed");

            await transaction.CommitAsync();
            return;
        }
        catch (DungeonEntranceErrorException errorEx)
        {
            PublishProcessEntranceError(
                sagaInfo: sagaInfo,
                dto: dto,
                errorMessage: errorEx.Message
            );
        }
        catch (RabbitMqException rex)
        {
            LogCritical(sagaInfo, dto, rex);
        }

        await transaction.RollbackAsync();
    }

    private async Task ConsumeChargeFee(DungeonEntranceGameDto dto, SagaInfo sagaInfo, IBasicProperties props)
    {
        if (dto.DungeonCost == null)
            throw new DungeonEntranceErrorException($"Field {nameof(dto.DungeonCost)} should not be null");

        var entranceGuid = dto.DungeonEntranceTransactionId;

        var dungeonEntrance = await GetDungeonEntranceByTransactionId(entranceGuid);

        if (dungeonEntrance == null)
            throw new DungeonEntranceErrorException("Entrance not found");

        if (dungeonEntrance.Character.Gold - dto.DungeonCost < 0)
            throw new DungeonEntranceErrorException($"Not enough gold, dungeon fee cost is {dto.DungeonCost} gold");

        dungeonEntrance.Character.Gold -= dto.DungeonCost.Value;
        dungeonEntrance.PayedFee = dto.DungeonCost.Value;
        dungeonEntrance.Status = DungeonEntranceStatusEnum.ReadyToUse;

        _dbContext.DungeonEntrances.Update(dungeonEntrance);

        if (await _dbContext.SaveChangesAsync() <= 0)
            throw new DungeonEntranceErrorException($"Entrance status could not be updated to {DungeonEntranceStatusEnum.ReadyToUse}");

        var @event = new DungeonEntranceArmoryDto
        {
            DungeonEntranceEvent = DungeonEntranceEventEnum.ProcessEntrance,
            DungeonTransactionId = dungeonEntrance.DungeonTransactionId,
            CharacterTransactionId = dungeonEntrance.Character.TransactionId,
            DungeonEntranceTransactionId = dungeonEntrance.TransactionId,
        };

        LogInformation(
            sagaInfo,
            dto,
            $"Character {dungeonEntrance.Character.TransactionId} successfully payed {dto.DungeonCost.Value} gold"
        );

        _dungeonEntranceProducer.Publish(@event, props);
    }

    private async Task ConsumeRollbackChargeFee(DungeonEntranceGameDto dto, SagaInfo sagaInfo)
    {
        var entranceGuid = dto.DungeonEntranceTransactionId;
        var entrance = await GetDungeonEntranceByTransactionId(entranceGuid);

        if (entrance == null)
            throw new DungeonEntranceErrorException("Entrance not found");

        if (entrance.PayedFee == null)
            throw new DungeonEntranceErrorException("Entrance must have a payed fee");

        entrance.Character.Gold += entrance.PayedFee.Value;
        _dbContext.Characters.Update(entrance.Character);

        if (await _dbContext.SaveChangesAsync() <= 0)
            throw new DungeonEntranceErrorException($"Character {entrance.Character.TransactionId} couldn't be refunded");

        LogInformation(sagaInfo, dto, $"Character with uuid {entrance.Character.TransactionId} was refunded successfully");
    }

    private async Task ConsumeRollbackEntrance(DungeonEntranceGameDto dto, SagaInfo sagaInfo)
    {
        var dungeonEntranceTransactionId = dto.DungeonEntranceTransactionId;
        var dungeonEntrance = await GetDungeonEntranceByTransactionId(dungeonEntranceTransactionId);

        if (dungeonEntrance == null)
            throw new Exception($"Entrance {dungeonEntranceTransactionId} not found");

        dungeonEntrance.Status = DungeonEntranceStatusEnum.RegistrationFailed;
        dungeonEntrance.Deleted = true;

        _dbContext.DungeonEntrances.Update(dungeonEntrance);

        if (await _dbContext.SaveChangesAsync() <= 0)
            throw new RabbitMqException($"Entrance status could not be updated to {DungeonEntranceStatusEnum.RegistrationFailed}");

        LogInformation(sagaInfo, dto, "Entrance rollback processed successfully and entity was deleted");
    }

    private Task<DungeonEntrances?> GetDungeonEntranceByTransactionId(Guid transactionId)
    {
        return _dbContext
               .DungeonEntrances
               .Include(x => x.Character)
               .FirstOrDefaultAsync(x => x.TransactionId == transactionId);
    }

    private void PublishProcessEntranceError(SagaInfo sagaInfo, DungeonEntranceGameDto dto, string errorMessage)
    {
        var @event = new DungeonEntranceArmoryDto
        {
            DungeonEntranceEvent = DungeonEntranceEventEnum.ProcessEntranceError,
            DungeonEntranceTransactionId = dto.DungeonEntranceTransactionId,
        };

        LogInformation(sagaInfo, dto, $"Sending {@event.DungeonEntranceEvent} event to game queue. Error message: {errorMessage}");

        _dungeonEntranceProducer.Publish(@event, sagaInfo);
    }

    private void LogInformation(SagaInfo sagaInfo, DungeonEntranceGameDto dto, string message)
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

    private void LogCritical(SagaInfo sagaInfo, DungeonEntranceGameDto dto, Exception ex)
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
