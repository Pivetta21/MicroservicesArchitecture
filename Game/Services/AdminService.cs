using AutoMapper;
using FluentResults;
using Game.Data;
using Game.Models;
using Game.Services.Interfaces;
using Game.ViewModels;

namespace Game.Services;

public class AdminService : IAdminService
{
    private readonly ILogger<AdminService> _logger;
    private readonly IMapper _mapper;
    private readonly GameDbContext _dbContext;

    public AdminService(
        ILogger<AdminService> logger,
        IMapper mapper,
        GameDbContext dbContext
    )
    {
        _logger = logger;
        _mapper = mapper;
        _dbContext = dbContext;
    }

    public async Task<Result<DungeonViewModel>> Create(DungeonCreateViewModel createViewModel)
    {
        var entity = _mapper.Map<Dungeons>(createViewModel);

        await _dbContext.AddAsync(entity);

        var writtenEntries = await _dbContext.SaveChangesAsync();

        if (writtenEntries <= 0)
            return Result.Fail("Could not create a new dungeon.");

        _logger.LogInformation("Dungeon '{@Dungeon}' created successfully", entity);
        return Result.Ok(_mapper.Map<DungeonViewModel>(entity));
    }
}
