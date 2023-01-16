using Common.DTOs.DungeonEntrance;
using FluentResults;
using Game.ViewModels;

namespace Game.Services.Interfaces;

public interface IDungeonEntranceService
{
    Task<Result<DungeonEntranceViewModel>> GetById(long id);

    Task ProcessDungeonEntrance(DungeonEntranceArmoryDto dto);
}
