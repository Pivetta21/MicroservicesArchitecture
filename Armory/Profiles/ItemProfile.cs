using Armory.Models;
using Armory.ViewModels;
using AutoMapper;

namespace Armory.Profiles;

public class ItemProfile : Profile
{
    public ItemProfile()
    {
        EntityToViewModel();
    }

    private void EntityToViewModel()
    {
        CreateMap<Armors, ArmorViewModel>()
            .ForMember(
                destinationMember: dest => dest.RarityDescription,
                memberOptions: opt => opt.MapFrom(src => src.Rarity.ToString())
            );

        CreateMap<Weapons, WeaponViewModel>()
            .ForMember(
                destinationMember: dest => dest.RarityDescription,
                memberOptions: opt => opt.MapFrom(src => src.Rarity.ToString())
            );
    }
}
