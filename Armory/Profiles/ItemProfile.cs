using Armory.Models;
using Armory.ViewModels;
using AutoMapper;

namespace Armory.Profiles;

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
            .ForMember(
                destinationMember: dest => dest.RarityDescription,
                memberOptions: opt => opt.MapFrom(src => src.Rarity.ToString())
            );

        CreateMap<Armors, ArmorViewModel>()
            .IncludeBase<Items, ItemViewModel>();

        CreateMap<Weapons, WeaponViewModel>()
            .IncludeBase<Items, ItemViewModel>();
    }

    private void ViewModelToEntity()
    {
        CreateMap<ItemCreateViewModel, Items>()
            .IncludeAllDerived();

        CreateMap<ArmorCreateViewModel, Armors>();

        CreateMap<WeaponCreateViewModel, Weapons>();
    }
}
