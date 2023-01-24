using Armory.Services.Interfaces;
using Armory.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Armory.Controllers;

[ApiController]
[Route("character")]
public class CharacterController : ControllerBase
{
    private readonly ICharacterService _characterService;

    public CharacterController(ICharacterService characterService)
    {
        _characterService = characterService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CharacterViewModel>>> Get()
    {
        var response = await _characterService.GetAll();
        return Ok(response);
    }

    [HttpGet("{transactionId:guid}")]
    public async Task<ActionResult<CharacterViewModel>> GetOne(Guid transactionId)
    {
        var result = await _characterService.GetByTransactionId(transactionId);
        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<CharacterViewModel>> Create(CharacterCreateViewModel body)
    {
        var result = await _characterService.Create(body);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetOne), new { result.Value.TransactionId }, result.Value);

        var errorResponse = result.Errors.Select(e => new { e.Message });
        return BadRequest(errorResponse);
    }

    [HttpPatch("{transactionId:guid}")]
    public async Task<IActionResult> Update(Guid transactionId, CharacterUpdateViewModel body)
    {
        var result = await _characterService.Update(transactionId, body);
        return result.IsSuccess ? NoContent() : NotFound(result.Errors.Select(e => new { e.Message }));
    }

    [HttpDelete("{transactionId:guid}")]
    public async Task<IActionResult> Delete(Guid transactionId)
    {
        var result = await _characterService.Delete(transactionId);
        return result.IsSuccess ? NoContent() : NotFound(result.Errors.Select(e => new { e.Message }));
    }

    [HttpPatch("{transactionId:guid}/action/sell-item")]
    public async Task<ActionResult<InventoryViewModel>> SellItem(Guid transactionId, CharacterActionViewModel body)
    {
        var result = await _characterService.SellItem(transactionId, body.ItemId);

        if (result.IsSuccess)
            return Ok(result.Value);

        var errorResponse = result.Errors.Select(e => new { e.Message });
        return BadRequest(errorResponse);
    }

    [HttpPatch("{transactionId:guid}/action/equip-item")]
    public async Task<ActionResult<BuildViewModel>> EquipItem(Guid transactionId, CharacterActionViewModel body)
    {
        var result = await _characterService.EquipItem(transactionId, body.ItemId);

        if (result.IsSuccess)
            return Ok(result.Value);

        var errorResponse = result.Errors.Select(e => new { e.Message });
        return BadRequest(errorResponse);
    }
}
