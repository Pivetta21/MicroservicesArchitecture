using Armory.Services.Interfaces;
using Armory.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Armory.Controllers;

[ApiController]
[Route("game")]
public class GameController : ControllerBase
{
    private readonly IDungeonEntranceService _dungeonEntranceService;

    public GameController(IDungeonEntranceService dungeonEntranceService)
    {
        _dungeonEntranceService = dungeonEntranceService;
    }

    [HttpPost("register-entrance")]
    public async Task<IActionResult> RegisterEntrance(DungeonRegisterEntranceViewModel body)
    {
        var result = await _dungeonEntranceService.RegisterEntrance(body);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Errors.Select(e => new { e.Message }));
    }
}
