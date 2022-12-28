using Game.Services;
using Game.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Game.Controllers;

[ApiController]
[Route("dungeon")]
public class DungeonController : ControllerBase
{
    private readonly IDungeonService _dungeonService;

    public DungeonController(IDungeonService dungeonService)
    {
        _dungeonService = dungeonService;
    }

    [HttpPost("register-entrance")]
    public async Task<ActionResult<DungeonEntranceViewModel>> RegisterEntrance(DungeonEntranceCreateViewModel body)
    {
        var result = await _dungeonService.RegisterEntrance(body);

        if (result.IsSuccess)
            return Ok(result.Value);

        var errorResponse = new
        {
            Errors = result.Errors.Select(e => new { e.Message }),
        };

        return BadRequest(errorResponse);
    }
}
