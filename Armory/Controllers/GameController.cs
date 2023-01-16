using Armory.Services.Interfaces;
using Armory.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Armory.Controllers;

[ApiController]
[Route("game")]
public class GameController : ControllerBase
{
    private readonly ILogger<GameController> _logger;
    private readonly IDungeonEntranceService _dungeonEntranceService;

    public GameController(
        ILogger<GameController> logger,
        IDungeonEntranceService dungeonEntranceService
    )
    {
        _logger = logger;
        _dungeonEntranceService = dungeonEntranceService;
    }

    [HttpPost("register-entrance")]
    public async Task<IActionResult> RegisterEntrance(DungeonRegisterEntranceViewModel body)
    {
        _logger.LogInformation(
            "Character with uuid {} wants to register an entrance for a dungeon with uuid {}",
            body.CharacterTransactionId,
            body.DungeonTransactionId
        );

        var result = await _dungeonEntranceService.RegisterEntrance(body);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Errors.Select(e => new { e.Message }));
    }
}
