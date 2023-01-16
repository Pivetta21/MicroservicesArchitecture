using FluentResults;
using Game.ViewModels;

namespace Game.Services.Interfaces;

public interface IAdminService
{
    Task<Result<DungeonViewModel>> Create(DungeonCreateViewModel createViewModel);
}
