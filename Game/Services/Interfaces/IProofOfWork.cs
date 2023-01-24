namespace Game.Services.Interfaces;

public interface IProofOfWork
{
    ValueTask<bool> FindHash(int difficulty);
}
