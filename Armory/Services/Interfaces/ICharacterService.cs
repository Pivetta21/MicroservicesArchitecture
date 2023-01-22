using Armory.ViewModels;
using FluentResults;

namespace Armory.Services.Interfaces;

public interface ICharacterService
{
    Task<IEnumerable<CharacterViewModel>> GetAll();

    Task<Result<CharacterViewModel>> GetByTransactionId(Guid transactionId);

    Task<Result<CharacterViewModel>> Create(CharacterCreateViewModel createViewModel);

    Task<Result> Update(Guid transactionId, CharacterUpdateViewModel updateViewModel);

    Task<Result> Delete(Guid transactionId);

    Task<Result<InventoryViewModel>> SellItem(Guid characterTransactionId, long itemId);

    Task<Result<BuildViewModel>> EquipItem(Guid transactionId, long itemId);
}
