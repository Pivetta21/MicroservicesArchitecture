using Armory.ViewModels;
using Common.DTOs.DungeonEntrance;
using FluentResults;

namespace Armory.Services.Interfaces;

public interface IDungeonEntranceService
{
    Task<Result<string>> RegisterEntrance(DungeonRegisterEntranceViewModel body, Guid dungeonTransactionId);

    Task ProcessDungeonEntrance(DungeonEntranceGameDto dto);
}
