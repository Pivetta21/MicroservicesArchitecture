using Armory.Services.Interfaces;
using Armory.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Armory.Controllers;

[ApiController]
[Route("admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _characterService;

    public AdminController(IAdminService characterService)
    {
        _characterService = characterService;
    }

    [HttpGet("item")]
    public async Task<ActionResult<IEnumerable<ItemViewModel>>> Get()
    {
        var response = await _characterService.GetAll();
        return Ok(response);
    }

    [HttpPost("item")]
    public async Task<ActionResult<ItemViewModel>> Create(ItemCreateViewModel body)
    {
        var result = await _characterService.Create(body);

        if (result.IsSuccess)
            return Ok(result.Value);

        var errorResponse = result.Errors.Select(e => new { e.Message });
        return BadRequest(errorResponse);
    }

    [HttpPost("add-item")]
    public async Task<ActionResult<InventoryViewModel>> AddItemToCharacterInventory(AddItemToCharacterViewModel body)
    {
        var result = await _characterService.AddItemToCharacter(body);

        if (result.IsSuccess)
            return Ok(result.Value);

        var errorResponse = result.Errors.Select(e => new { e.Message });
        return BadRequest(errorResponse);
    }
}
