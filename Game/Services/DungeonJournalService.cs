using AutoMapper;
using FluentResults;
using Game.Data;
using Game.Services.Interfaces;
using Game.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Game.Services;

public class DungeonJournalService : IDungeonJournalService
{
    private readonly IMapper _mapper;
    private readonly GameDbContext _dbContext;

    public DungeonJournalService(
        IMapper mapper,
        GameDbContext dbContext
    )
    {
        _mapper = mapper;
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<DungeonJournalViewModel>> GetAll()
    {
        var entrances = await _dbContext.DungeonJournals
                                        .Include(e => e.Dungeon)
                                        .Include(e => e.Reward)
                                        .OrderByDescending(e => e.Id)
                                        .ToListAsync();

        return _mapper.Map<IEnumerable<DungeonJournalViewModel>>(entrances);
    }

    public async Task<Result<DungeonJournalViewModel>> GetById(long id)
    {
        var entrance = await _dbContext.DungeonJournals
                                       .Include(e => e.Dungeon)
                                       .Include(e => e.Reward)
                                       .FirstOrDefaultAsync(x => x.Id == id);

        return entrance == null
            ? Result.Fail($"A dungeon journal with id equal to '{id}' could not be found")
            : Result.Ok(_mapper.Map<DungeonJournalViewModel>(entrance));
    }
}
