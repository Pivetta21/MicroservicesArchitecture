using Armory.ViewModels;
using FluentResults;

namespace Armory.Services.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<ItemViewModel>> GetAll();

    Task<Result<ItemViewModel>> Create(ItemCreateViewModel createViewModel);

    Task<Result<InventoryViewModel>> AddItemToCharacter(AddItemToCharacterViewModel addItemViewModel);
}
