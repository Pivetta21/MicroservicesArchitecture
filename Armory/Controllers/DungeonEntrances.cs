using Armory.Models.Enums;
using Armory.Services.Interfaces;
using Armory.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Armory.Controllers;

[ApiController]
[Route("dungeon-entrance")]
public class DungeonEntrances : ControllerBase
{
    private readonly IDungeonEntranceService _dungeonEntranceService;

    public DungeonEntrances(IDungeonEntranceService dungeonEntranceService)
    {
        _dungeonEntranceService = dungeonEntranceService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DungeonEntranceViewModel>>> GetByCharacter(
        long? characterId,
        DungeonEntranceStatusEnum? status
    )
    {
        var response = await _dungeonEntranceService.Get(characterId, status);
        return Ok(response);
    }
}
