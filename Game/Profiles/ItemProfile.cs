using AutoMapper;
using Game.Models;
using Game.ViewModels;

namespace Game.Profiles;

public class ItemProfile : Profile
{
    public ItemProfile()
    {
        EntityToViewModel();
        ViewModelToEntity();
    }

    private void EntityToViewModel()
    {
        CreateMap<Items, ItemViewModel>()
            .IncludeAllDerived();

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

    private void ViewModelToEntity()
    {
        CreateMap<ItemCreateViewModel, Items>()
            .IncludeAllDerived();

        CreateMap<ArmorCreateViewModel, Armors>();

        CreateMap<WeaponCreateViewModel, Weapons>();
    }
}
