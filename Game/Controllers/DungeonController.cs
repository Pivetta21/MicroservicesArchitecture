using Game.Services.Interfaces;
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DungeonViewModel>>> Get()
    {
        var response = await _dungeonService.GetAll();
        return Ok(response);
    }

    [HttpGet("{transactionId:guid}")]
    public async Task<ActionResult<DungeonViewModel>> GetOne(Guid transactionId)
    {
        var result = await _dungeonService.GetByTransactionId(transactionId);
        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }
}
