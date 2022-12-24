using Game.Services;
using Microsoft.AspNetCore.Mvc;

namespace Game.Controllers;

[ApiController]
[Route("dungeon")]
public class DungeonController : ControllerBase
{
    private readonly ILogger<DungeonController> _logger;
    private readonly IDungeonService _dungeonService;

    public DungeonController(
        ILogger<DungeonController> logger,
        IDungeonService dungeonService
    )
    {
        _logger = logger;
        _dungeonService = dungeonService;
    }
}
