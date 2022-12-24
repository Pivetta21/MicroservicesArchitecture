namespace Game.Services;

public interface IProofOfWork
{
    ValueTask<bool> FindHash(int difficulty);
}
