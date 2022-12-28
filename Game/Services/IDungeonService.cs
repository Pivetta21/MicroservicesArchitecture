using FluentResults;
using Game.ViewModels;

namespace Game.Services;

public interface IDungeonService
{
    Task<DungeonResultViewModel> Play(int difficulty);

    Task<Result<DungeonEntranceViewModel>> RegisterEntrance(DungeonEntranceCreateViewModel createViewModel);
}
