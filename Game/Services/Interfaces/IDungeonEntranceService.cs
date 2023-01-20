using Common.DTOs.DungeonEntrance;
using FluentResults;
using Game.ViewModels;

namespace Game.Services.Interfaces;

public interface IDungeonEntranceService
{
    Task<IEnumerable<DungeonEntranceViewModel>> GetAll();

    Task<Result<DungeonEntranceViewModel>> GetByTransactionId(Guid transactionId);

    Task ProcessDungeonEntrance(DungeonEntranceArmoryDto dto);
}
