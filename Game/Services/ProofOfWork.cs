using Game.Services.Interfaces;

namespace Game.Services;

public class ProofOfWork : IProofOfWork
{
    public async ValueTask<bool> FindHash(int difficulty)
    {
        Thread.Sleep(TimeSpan.FromSeconds(difficulty));
        return await new ValueTask<bool>(true);
    }
}
