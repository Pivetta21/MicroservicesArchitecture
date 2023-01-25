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
    public async Task<ActionResult<DungeonEntranceViewModel>> RegisterEntrance(DungeonRegisterEntranceViewModel body)
    {
        var result = await _dungeonEntranceService.RegisterEntrance(body);

        if (!result.IsSuccess)
            return NotFound(result.Errors.Select(e => new { e.Message }));

        return Ok(result.Value);
    }

    [HttpPost("play-dungeon")]
    public async Task<ActionResult<DungeonEntranceViewModel>> PlayDungeon(PlayDungeonViewModel body)
    {
        var result = await _dungeonService.PlayDungeon(body);

        if (!result.IsSuccess)
            return NotFound(result.Errors.Select(e => new { e.Message }));

        return Ok(result.Value);
    }
}
