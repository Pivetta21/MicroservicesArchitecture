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

    public async Task<Result<DungeonEntranceViewModel>> GetById(long id)
    {
        var entrance = await _dbContext
                             .DungeonEntrances
                             .FirstOrDefaultAsync(x => x.Id == id);

        if (entrance == null)
            return Result.Fail($"A dungeon entrance with id equal to '{id}' could not be found");

        _logger.LogInformation("Dungeon entrance '{}' found successfully", id);

        return Result.Ok(_mapper.Map<DungeonEntranceViewModel>(entrance));
    }

    public Task ProcessDungeonEntrance(DungeonEntranceArmoryDto dto)
    {
        return dto.DungeonEntranceEvent switch
        {
            DungeonEntranceEventEnum.RegisterEntrance => ConsumeRegisterEntrance(dto),
            DungeonEntranceEventEnum.ProcessEntrance => ConsumeProcessEntrance(dto),
            DungeonEntranceEventEnum.ProcessEntranceError => ConsumeProcessEntranceError(dto),
            _ => Task.CompletedTask,
        };
    }

    private async Task ConsumeRegisterEntrance(DungeonEntranceArmoryDto dto)
    {
        var dungeon = await _dbContext
                            .Dungeons
                            .FirstOrDefaultAsync(d => d.TransactionId == dto.DungeonTransactionId);

        if (dungeon == null || dto.CharacterTransactionId == null)
        {
            PublishRollbackEntrance(
                dungeonEntranceTransactionId: dto.DungeonEntranceTransactionId,
                errorMessage: dto.CharacterTransactionId == null
                    ? $"Field {nameof(dto.CharacterTransactionId)} should not be null"
                    : $"Dungeon with uuid {dto.DungeonTransactionId} not found"
            );

            return;
        }

        var dungeonEntrance = new DungeonEntrances
        {
            CharacterTransactionId = dto.CharacterTransactionId!.Value,
            TransactionId = dto.DungeonEntranceTransactionId,
            Processed = false,
            Dungeon = dungeon,
        };

        _dbContext.DungeonEntrances.Add(dungeonEntrance);

        var writtenEntries = await _dbContext.SaveChangesAsync();

        if (writtenEntries <= 0)
        {
            PublishRollbackEntrance(
                dungeonEntranceTransactionId: dto.DungeonEntranceTransactionId,
                errorMessage: $"Dungeon entrance {dto.DungeonTransactionId} could not be created"
            );

            return;
        }

        _logger.LogInformation(
            "Dungeon entrance {} persisted successfully, sending {} event to Armory queue",
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
        {
            PublishRollbackChargeFee(
                dungeonEntranceTransactionId: dto.DungeonEntranceTransactionId,
                errorMessage: $"Dungeon with uuid {dto.DungeonTransactionId} not found"
            );

            return;
        }

        dungeonEntrance.Processed = true;

        _dbContext.DungeonEntrances.Update(dungeonEntrance);
        var writtenEntries = await _dbContext.SaveChangesAsync();

        if (writtenEntries <= 0)
        {
            PublishRollbackChargeFee(
                dungeonEntranceTransactionId: dungeonEntrance.TransactionId,
                errorMessage: $"Dungeon entrance with uuid {dto.DungeonTransactionId} could not be marked as processed"
            );
        }
        else
        {
            _logger.LogInformation(
                "Dungeon entrance {} processed successfully",
                dungeonEntrance.TransactionId
            );
        }
    }

    private async Task ConsumeProcessEntranceError(DungeonEntranceArmoryDto dto)
    {
        var dungeonEntrance = await GetDungeonEntranceByTransactionId(dto.DungeonEntranceTransactionId);

        if (dungeonEntrance == null)
        {
            PublishRollbackEntrance(
                dungeonEntranceTransactionId: dto.DungeonEntranceTransactionId,
                errorMessage: $"Dungeon entrance not found {dto.DungeonEntranceTransactionId}"
            );

            return;
        }

        _dbContext.DungeonEntrances.Remove(dungeonEntrance);
        var writtenEntries = await _dbContext.SaveChangesAsync();

        if (writtenEntries <= 0)
        {
            _logger.LogCritical(
                "Dungeon entrance {} could not be removed",
                dungeonEntrance.TransactionId
            );
        }

        PublishRollbackEntrance(
            dungeonEntranceTransactionId: dto.DungeonEntranceTransactionId,
            errorMessage: $"Dungeon entrance {dungeonEntrance.TransactionId} with error was successfully removed"
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
            "{}, sending {} event to Armory queue",
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
            "{}, sending {} to Armory queue",
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
