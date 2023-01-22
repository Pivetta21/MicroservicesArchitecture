using Armory.Models;
using Armory.ViewModels;
using AutoMapper;

namespace Armory.Profiles;

public class CharacterProfile : Profile
{
    public CharacterProfile()
    {
        EntityToViewModel();
        ViewModelToEntity();
    }

    private void EntityToViewModel()
    {
        CreateMap<Builds, BuildViewModel>();

        CreateMap<Inventories, InventoryViewModel>();

        CreateMap<Builds, BuildViewModel>();

        CreateMap<Characters, CharacterViewModel>()
            .ForMember(
                destinationMember: dest => dest.SpecializationDescription,
                memberOptions: opt => opt.MapFrom(src => src.Specialization.ToString())
            );
    }

    private void ViewModelToEntity()
    {
        CreateMap<CharacterUpdateViewModel, Characters>();
    }
}
