using System.Diagnostics;
using AutoMapper;
using Game.Data;
using Game.ViewModels;

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
}
