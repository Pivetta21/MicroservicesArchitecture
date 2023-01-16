using Game.ViewModels;

namespace Game.Services.Interfaces;

public interface IDungeonService
{
    Task<DungeonResultViewModel> Play(int difficulty);
}
