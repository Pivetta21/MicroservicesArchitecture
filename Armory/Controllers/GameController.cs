using Armory.Services.Interfaces;
using Armory.ViewModels;
using Common.DTOs.DungeonEntrance;
using Common.DTOs.PlayDungeon;
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
    public async Task<ActionResult<DungeonEntranceChoreographySagaDto>> RegisterEntrance(DungeonRegisterEntranceViewModel body)
    {
        var dungeonTransactionId = Guid.NewGuid();

        var result = await _dungeonEntranceService.RegisterEntrance(body, dungeonTransactionId);

        if (!result.IsSuccess)
            return NotFound(result.Errors.Select(e => new { e.Message }));

        var response = new DungeonEntranceChoreographySagaDto
        {
            Message = result.Value,
            DungeonEntranceTransactionId = dungeonTransactionId,
        };

        return Ok(response);
    }

    [HttpPost("play-dungeon")]
    public async Task<ActionResult<PlayDungeonOrchestrationSagaDto>> PlayDungeon(PlayDungeonViewModel body)
    {
        var result = await _dungeonService.PlayDungeon(body);

        if (!result.IsSuccess)
            return NotFound(result.Errors.Select(e => new { e.Message }));

        var response = new PlayDungeonOrchestrationSagaDto
        {
            Message = "Your play dungeon request was sent successfully and will be processed soon",
            DungeonEntranceTransactionId = result.Value.DungeonTransactionId,
            DungeonEntranceStatus = result.Value.StatusDescription,
        };

        return Ok(response);
    }
}
