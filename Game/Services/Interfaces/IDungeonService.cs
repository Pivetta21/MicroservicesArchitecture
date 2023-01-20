using FluentResults;
using Game.ViewModels;

namespace Game.Services.Interfaces;

public interface IDungeonService
{
    Task<DungeonResultViewModel> Play(int difficulty);

    Task<IEnumerable<DungeonViewModel>> GetAll();

    Task<Result<DungeonViewModel>> GetByTransactionId(Guid transactionId);
}
