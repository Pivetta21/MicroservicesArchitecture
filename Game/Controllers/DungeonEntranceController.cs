using Game.Services.Interfaces;
using Game.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Game.Controllers;

[ApiController]
[Route("dungeon-entrance")]
public class DungeonEntranceController : ControllerBase
{
    private readonly IDungeonEntranceService _dungeonEntranceService;

    public DungeonEntranceController(IDungeonEntranceService dungeonEntranceService)
    {
        _dungeonEntranceService = dungeonEntranceService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DungeonEntranceViewModel>>> Get()
    {
        var response = await _dungeonEntranceService.GetAll();
        return Ok(response);
    }

    [HttpGet("{transactionId:guid}")]
    public async Task<ActionResult<DungeonEntranceViewModel>> GetOne(Guid transactionId)
    {
        var result = await _dungeonEntranceService.GetByTransactionId(transactionId);
        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }
}
