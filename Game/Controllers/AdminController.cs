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

    [HttpPost("dungeon")]
    public async Task<IActionResult> CreateDungeon(DungeonCreateViewModel body)
    {
        var result = await _adminService.Create(body);

        if (result.IsSuccess)
            return Ok(result.Value);

        var errorResponse = result.Errors.Select(e => new { e.Message });
        return BadRequest(errorResponse);
    }
}
