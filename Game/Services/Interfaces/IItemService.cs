using Common.DTOs.Item;
using FluentResults;
using Game.ViewModels;

namespace Game.Services.Interfaces;

public interface IItemService
{
    Task<IEnumerable<ItemViewModel>> GetAll();

    Task<Result<ItemViewModel>> GetByTransactionId(Guid transactionId);

    Task<Result<ItemPriceDto>> GetPriceByTransactionId(Guid transactionId);
}
