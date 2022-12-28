using System.Diagnostics;
using AutoMapper;
using FluentResults;
using Game.Data;
using Game.Models;
using Game.Models.Enums;
using Game.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Game.Services;

public class DungeonService : IDungeonService
{
    private readonly IProofOfWork _proofOfWork;
    private readonly ILogger<DungeonService> _logger;
    private readonly IMapper _mapper;
    private readonly GameDbContext _dbContext;

    public DungeonService(
        IProofOfWork proofOfWork,
        ILogger<DungeonService> logger,
        IMapper mapper,
        GameDbContext dbContext
    )
    {
        _proofOfWork = proofOfWork;
        _logger = logger;
        _mapper = mapper;
        _dbContext = dbContext;
    }

    public async Task<DungeonResultViewModel> Play(int difficulty)
    {
        var stopwatch = new Stopwatch();

        stopwatch.Start();

        var hashFound = await _proofOfWork.FindHash(difficulty);

        stopwatch.Stop();

        return new DungeonResultViewModel
        {
            Success = hashFound,
            Time = stopwatch.ElapsedMilliseconds,
        };
    }

    public async Task<Result<DungeonEntranceViewModel>> RegisterEntrance(DungeonEntranceCreateViewModel createViewModel)
    {
        var dungeon = await _dbContext.Dungeons.FirstOrDefaultAsync(d => d.Id == createViewModel.DungeonId);

        if (dungeon == null)
            return Result.Fail($"Dungeon '{createViewModel.DungeonId}' could not be found.");

        var entity = new DungeonEntrances
        {
            CharacterTransactionId = createViewModel.CharacterTransactionId,
            DungeonId = createViewModel.DungeonId,
            Processed = false,
            Status = DungeonEntranceStatusEnum.Created,
            Dungeon = dungeon,
        };

        _dbContext.DungeonEntrances.Add(entity);
        var writtenEntries = await _dbContext.SaveChangesAsync();

        if (writtenEntries > 0)
        {
            _logger.LogInformation("Sending entrance '{}' to ProcessDungeonRegistrationQueue", entity.Id);
            return Result.Ok(_mapper.Map<DungeonEntranceViewModel>(entity));
        }

        _logger.LogInformation(
            "Failed to register a new entrance in {} dungeon for user '{}'",
            dungeon.Name,
            createViewModel.CharacterTransactionId
        );

        return Result.Fail($"Could not register a new dungeon entrance.");
    }
}
