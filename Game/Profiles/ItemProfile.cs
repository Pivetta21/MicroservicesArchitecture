using AutoMapper;
using Common.DTOs.Item;
using Game.Models;
using Game.ViewModels;

namespace Game.Profiles;

public class ItemProfile : Profile
{
    public ItemProfile()
    {
        EntityToDto();
        EntityToViewModel();
        ViewModelToEntity();
    }

    private void EntityToDto()
    {
        CreateMap<Items, ItemPriceDto>();
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
