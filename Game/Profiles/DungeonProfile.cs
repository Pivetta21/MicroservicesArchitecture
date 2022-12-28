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
        CreateMap<DungeonEntrances, DungeonEntranceViewModel>()
            .ForMember(
                destinationMember: dest => dest.StatusDescription,
                memberOptions: opt => opt.MapFrom(src => src.Status.ToString())
            );
    }

    private void ViewModelToEntity()
    {
        CreateMap<DungeonCreateViewModel, Dungeons>();
    }
}
