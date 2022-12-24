using AutoMapper;
using Game.Models;
using Game.ViewModels;

namespace Game.Profiles;

public class DungeonProfile : Profile
{
    public DungeonProfile()
    {
        EntityToViewModel();
        ViewModelToEntity();
    }

    private void EntityToViewModel()
    {
        CreateMap<Dungeons, DungeonViewModel>();
    }

    private void ViewModelToEntity()
    {
        CreateMap<DungeonCreateViewModel, Dungeons>();
    }
}
