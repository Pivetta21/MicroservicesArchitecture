using Common.DTOs.Item;
using Game.Services.Interfaces;
using Game.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Game.Controllers;

[ApiController]
[Route("item")]
public class ItemController : ControllerBase
{
    private readonly IItemService _itemService;

    public ItemController(IItemService itemService)
    {
        _itemService = itemService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemViewModel>>> Get()
    {
        var response = await _itemService.GetAll();
        return Ok(response);
    }

    [HttpGet("{transactionId:guid}")]
    public async Task<ActionResult<ItemViewModel>> GetOne(Guid transactionId)
    {
        var result = await _itemService.GetByTransactionId(transactionId);
        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }

    [HttpGet("{transactionId:guid}/price")]
    public async Task<ActionResult<ItemPriceDto>> GetPrice(Guid transactionId)
    {
        var result = await _itemService.GetPriceByTransactionId(transactionId);
        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }
}
