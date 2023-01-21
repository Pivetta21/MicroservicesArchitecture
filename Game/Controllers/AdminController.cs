using Game.Services.Interfaces;
using Game.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Game.Controllers;

[Route("admin")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("dungeon")]
    public async Task<ActionResult<IEnumerable<DungeonViewModel>>> Get()
    {
        var response = await _adminService.GetAll();
        return Ok(response);
    }

    [HttpPost("dungeon")]
    public async Task<IActionResult> CreateDungeon(DungeonCreateViewModel body)
    {
        var result = await _adminService.CreateDungeon(body);

        if (result.IsSuccess)
            return Ok(result.Value);

        var errorResponse = result.Errors.Select(e => new { e.Message });
        return BadRequest(errorResponse);
    }

    [HttpDelete("dungeon/{transactionId:guid}")]
    public async Task<IActionResult> DeleteDungeon(Guid transactionId)
    {
        var result = await _adminService.DeleteDungeon(transactionId);
        return result.IsSuccess ? NoContent() : NotFound(result.Errors.Select(e => new { e.Message }));
    }
}
