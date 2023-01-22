using Armory.Services.Interfaces;
using Armory.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Armory.Controllers;

[ApiController]
[Route("game")]
public class GameController : ControllerBase
{
    private readonly IDungeonEntranceService _dungeonEntranceService;
    private readonly IDungeonService _dungeonService;

    public GameController(
        IDungeonEntranceService dungeonEntranceService,
        IDungeonService dungeonService
    )
    {
        _dungeonEntranceService = dungeonEntranceService;
        _dungeonService = dungeonService;
    }

    [HttpPost("register-entrance")]
    public async Task<IActionResult> RegisterEntrance(DungeonRegisterEntranceViewModel body)
    {
        var result = await _dungeonEntranceService.RegisterEntrance(body);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Errors.Select(e => new { e.Message }));
    }

    [HttpPost("play-dungeon")]
    public async Task<IActionResult> PlayDungeon(PlayDungeonViewModel body)
    {
        var result = await _dungeonService.PlayDungeon(body);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Errors.Select(e => new { e.Message }));
    }
}
