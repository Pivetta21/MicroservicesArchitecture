using System.Diagnostics;
using Game.Services.Interfaces;

namespace Game.Services;

public class ProofOfWork : IProofOfWork
{
    private static readonly Random Generator = Random.Shared;

    public ValueTask<bool> FindHash(int difficulty)
    {
        var minDifficulty = Math.Floor(Math.Max(difficulty * 0.8, 1));
        var maxDifficulty = Math.Ceiling(difficulty * 1.4);
        var currentDifficulty = Generator.Next((int)minDifficulty, (int)maxDifficulty);

        Thread.Sleep(TimeSpan.FromSeconds(currentDifficulty));

        var hashFound = Generator.Next(int.MaxValue) % 2 == 0;

        return new ValueTask<bool>(hashFound);
    }
}
