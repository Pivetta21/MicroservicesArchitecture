namespace Game.Services.Interfaces;

public interface IProofOfWork
{
    Task<ProofOfWorkResult> FindHash(int difficulty);
}
