using Armory.Models;
using Armory.ViewModels;
using AutoMapper;

namespace Armory.Profiles;

public class DungeonProfile : Profile
{
    public DungeonProfile()
    {
        EntityToViewModel();
    }

    private void EntityToViewModel()
    {
        CreateMap<DungeonEntrances, DungeonEntranceViewModel>()
            .ForMember(
                destinationMember: dest => dest.StatusDescription,
                memberOptions: opt => opt.MapFrom(src => src.Status.ToString())
            );
    }
}
