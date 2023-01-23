using System.Diagnostics;
using Game.Services.Interfaces;

namespace Game.Services;

public class ProofOfWork : IProofOfWork
{
    private static readonly Random Generator = Random.Shared;

    public Task<ProofOfWorkResult> FindHash(int difficulty)
    {
        var stopwatch = new Stopwatch();

        stopwatch.Start();

        var minDifficulty = Math.Floor(difficulty * 0.8);
        var maxDifficulty = Math.Ceiling(difficulty * 1.4);
        var currentDifficulty = Generator.Next((int)minDifficulty, (int)maxDifficulty);

        Thread.Sleep(TimeSpan.FromSeconds(currentDifficulty));

        stopwatch.Stop();

        var result = new ProofOfWorkResult
        {
            Success = Generator.Next(int.MaxValue) % 2 == 0,
            Time = stopwatch.ElapsedMilliseconds,
        };

        return Task.FromResult(result);
    }
}

public class ProofOfWorkResult
{
    public bool Success { get; set; }

    public long Time { get; set; }
}
