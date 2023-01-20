using System.Diagnostics;
using AutoMapper;
using FluentResults;
using Game.Data;
using Game.Services.Interfaces;
using Game.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Game.Services;

public class DungeonService : IDungeonService
{
    private readonly IMapper _mapper;
    private readonly GameDbContext _dbContext;
    private readonly IProofOfWork _proofOfWork;


    public DungeonService(
        IMapper mapper,
        GameDbContext dbContext,
        IProofOfWork proofOfWork
    )
    {
        _mapper = mapper;
        _dbContext = dbContext;
        _proofOfWork = proofOfWork;
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

    public async Task<IEnumerable<DungeonViewModel>> GetAll()
    {
        var dungeons = await _dbContext.Dungeons
                                       .Include(d => d.Rewards)
                                       .OrderByDescending(d => d.Id)
                                       .ToListAsync();

        return _mapper.Map<IEnumerable<DungeonViewModel>>(dungeons);
    }

    public async Task<Result<DungeonViewModel>> GetByTransactionId(Guid transactionId)
    {
        var dungeon = await _dbContext.Dungeons
                                      .Include(d => d.Rewards)
                                      .FirstOrDefaultAsync(a => a.TransactionId == transactionId);

        return dungeon == null
            ? Result.Fail<DungeonViewModel>($"A dungeon with uuid equal to '{transactionId}' could not be found")
            : Result.Ok(_mapper.Map<DungeonViewModel>(dungeon));
    }
}
