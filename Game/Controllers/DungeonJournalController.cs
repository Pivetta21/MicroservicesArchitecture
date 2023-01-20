using Game.Services.Interfaces;
using Game.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Game.Controllers;

[ApiController]
[Route("dungeon-journal")]
public class DungeonJournalController : ControllerBase
{
    private readonly IDungeonJournalService _dungeonJournalService;

    public DungeonJournalController(IDungeonJournalService dungeonJournalService)
    {
        _dungeonJournalService = dungeonJournalService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DungeonJournalViewModel>>> Get()
    {
        var response = await _dungeonJournalService.GetAll();
        return Ok(response);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<DungeonJournalViewModel>> GetOne(long id)
    {
        var result = await _dungeonJournalService.GetById(id);
        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }
}
