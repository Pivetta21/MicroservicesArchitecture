using AutoMapper;
using FluentResults;
using Game.Data;
using Game.Models;
using Game.Services.Interfaces;
using Game.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Game.Services;

public class AdminService : IAdminService
{
    private readonly IMapper _mapper;
    private readonly GameDbContext _dbContext;

    public AdminService(
        IMapper mapper,
        GameDbContext dbContext
    )
    {
        _mapper = mapper;
        _dbContext = dbContext;
    }

    public async Task<Result<DungeonViewModel>> CreateDungeon(DungeonCreateViewModel createViewModel)
    {
        var entity = _mapper.Map<Dungeons>(createViewModel);

        await _dbContext.AddAsync(entity);

        var writtenEntries = await _dbContext.SaveChangesAsync();

        return writtenEntries <= 0
            ? Result.Fail("Could not create a new dungeon.")
            : Result.Ok(_mapper.Map<DungeonViewModel>(entity));
    }

    public async Task<Result> DeleteDungeon(Guid transactionId)
    {
        var entity = await _dbContext.Dungeons
                                     .FirstOrDefaultAsync(d => d.TransactionId == transactionId);

        if (entity == null)
            return Result.Fail($"Dungeon '{transactionId}' not found.");

        _dbContext.Remove(entity);
        var writtenEntries = await _dbContext.SaveChangesAsync();

        return writtenEntries > 0 ? Result.Ok() : Result.Fail($"Could not delete dungeon '{transactionId}'");
    }
}
