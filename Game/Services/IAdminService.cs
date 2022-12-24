using FluentResults;
using Game.ViewModels;

namespace Game.Services;

public interface IAdminService
{
    Task<Result<DungeonViewModel>> Create(DungeonCreateViewModel createViewModel);
}
