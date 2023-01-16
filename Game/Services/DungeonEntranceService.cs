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
        switch (dto.DungeonEntranceEvent)
        {
            case DungeonEntranceEventEnum.RegisterEntrance:
                return RegisterEntrance(dto);
            case DungeonEntranceEventEnum.ProcessRegistration:
                return ProcessRegistration(dto);
            case DungeonEntranceEventEnum.ProcessChargeFeeError:
                return ProcessChargeFeeError(dto);
            case DungeonEntranceEventEnum.ChargeFee:
            case DungeonEntranceEventEnum.RollbackCreate:
            case DungeonEntranceEventEnum.RollbackChargeFee:
            default:
                return Task.CompletedTask;
        }
    }

    private async Task ProcessChargeFeeError(DungeonEntranceArmoryDto dto)
    {
        // TODO remove DungeonEntrance and if has error emit a critical log
    }

    private async Task ProcessRegistration(DungeonEntranceArmoryDto dto)
    {
        var dungeonEntrance = await _dbContext
                                    .DungeonEntrances
                                    .FirstOrDefaultAsync(d => d.TransactionId == dto.DungeonEntranceTransactionId);

        if (dungeonEntrance == null)
        {
            _logger.LogWarning(
                "Dungeon with uuid {} not found",
                dto.DungeonTransactionId
            );

            RollbackChargeFee(dto.DungeonEntranceTransactionId);
            return;
        }

        dungeonEntrance.Processed = true;

        _dbContext.DungeonEntrances.Update(dungeonEntrance);
        var writtenEntries = await _dbContext.SaveChangesAsync();

        if (writtenEntries <= 0)
        {
            _logger.LogWarning(
                "Dungeon entrance with uuid {} could not be marked as processed",
                dto.DungeonTransactionId
            );

            RollbackChargeFee(dungeonEntrance.TransactionId);
        }
        else
        {
            _logger.LogInformation(
                "Dungeon entrance {} processed successfully",
                dungeonEntrance.TransactionId
            );
        }
    }

    private void RollbackChargeFee(Guid dungeonEntranceTransactionId)
    {
        _dungeonEntranceProducer.Publish(
            @event: new DungeonEntranceGameDto
            {
                DungeonEntranceTransactionId = dungeonEntranceTransactionId,
                DungeonEntranceEvent = DungeonEntranceEventEnum.RollbackChargeFee,
            }
        );

        _logger.LogInformation(
            "Dungeon entrance with uuid {} could not be processed, sending {} to Armory queue",
            dungeonEntranceTransactionId,
            DungeonEntranceEventEnum.RollbackChargeFee
        );
    }

    private async Task RegisterEntrance(DungeonEntranceArmoryDto dto)
    {
        if (dto.CharacterTransactionId == null)
            throw new Exception($"Field {nameof(dto.CharacterTransactionId)} should not be null");

        var dungeon = await _dbContext
                            .Dungeons
                            .FirstOrDefaultAsync(d => d.TransactionId == dto.DungeonTransactionId);

        if (dungeon == null)
            throw new Exception($"Dungeon with uuid {dto.DungeonTransactionId} not found");

        var dungeonEntrance = new DungeonEntrances
        {
            CharacterTransactionId = dto.CharacterTransactionId.Value,
            TransactionId = dto.DungeonEntranceTransactionId,
            Processed = false,
            Dungeon = dungeon,
        };

        _dbContext.DungeonEntrances.Add(dungeonEntrance);

        var writtenEntries = await _dbContext.SaveChangesAsync();

        if (writtenEntries <= 0)
        {
            _logger.LogWarning(
                "Error to register a new dungeon entrance with uuid {} to user {}",
                dto.DungeonTransactionId,
                dto.CharacterTransactionId
            );

            RollbackEntrance(dto);
        }
        else
        {
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
    }

    private void RollbackEntrance(DungeonEntranceArmoryDto dto)
    {
        _dungeonEntranceProducer.Publish(
            @event: new DungeonEntranceGameDto
            {
                DungeonEntranceTransactionId = dto.DungeonEntranceTransactionId,
                DungeonEntranceEvent = DungeonEntranceEventEnum.RollbackCreate,
            }
        );

        _logger.LogInformation(
            "Failed to persist dungeon entrance {}, sending {} event to Armory queue",
            dto.DungeonEntranceTransactionId,
            DungeonEntranceEventEnum.RollbackCreate
        );
    }
}
