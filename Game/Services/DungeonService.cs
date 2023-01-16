using System.Diagnostics;
using Game.Services.Interfaces;
using Game.ViewModels;

namespace Game.Services;

public class DungeonService : IDungeonService
{
    private readonly IProofOfWork _proofOfWork;

    public DungeonService(IProofOfWork proofOfWork)
    {
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
}
