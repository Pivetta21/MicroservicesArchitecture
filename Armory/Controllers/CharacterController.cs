using Armory.Services.Interfaces;
using Armory.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Armory.Controllers;

[ApiController]
[Route("character")]
public class CharacterController : ControllerBase
{
    private readonly ILogger<CharacterController> _logger;
    private readonly ICharacterService _characterService;

    public CharacterController(
        ILogger<CharacterController> logger,
        ICharacterService characterService
    )
    {
        _logger = logger;
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
        _logger.LogDebug("Searching for a character with transaction id equal to '{}'", transactionId);

        var result = await _characterService.GetByTransactionId(transactionId);
        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<CharacterViewModel>> Create(CharacterCreateViewModel body)
    {
        _logger.LogDebug("Trying to create a {} named {}", body.Specialization.ToString(), body.Name);

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
}
