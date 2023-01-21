using FluentResults;
using Game.ViewModels;

namespace Game.Services.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<DungeonViewModel>> GetAll();

    Task<Result<DungeonViewModel>> CreateDungeon(DungeonCreateViewModel createViewModel);

    Task<Result> DeleteDungeon(Guid transactionId);
}
