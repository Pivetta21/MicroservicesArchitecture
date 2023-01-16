using Game.Services.Interfaces;
using Game.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Game.Controllers;

[Route("admin")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;
    private readonly IAdminService _adminService;

    public AdminController(
        ILogger<AdminController> logger,
        IAdminService adminService
    )
    {
        _logger = logger;
        _adminService = adminService;
    }

    [HttpPost("dungeon")]
    public async Task<IActionResult> CreateDungeon(DungeonCreateViewModel body)
    {
        _logger.LogDebug("Trying to create '{}' dungeon", body.Name);
        var result = await _adminService.Create(body);

        if (result.IsSuccess)
            return Ok(result.Value);

        var errorResponse = result.Errors.Select(e => new { e.Message });
        return BadRequest(errorResponse);
    }
}
