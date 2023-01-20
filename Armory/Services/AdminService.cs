using Armory.Data;
using Armory.Models;
using Armory.Services.Interfaces;
using Armory.ViewModels;
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Armory.Services;

public class AdminService : IAdminService
{
    private readonly IMapper _mapper;
    private readonly ArmoryDbContext _dbContext;

    public AdminService(IMapper mapper, ArmoryDbContext dbContext)
    {
        _mapper = mapper;
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<ItemViewModel>> GetAll()
    {
        var items = await _dbContext
                          .Items
                          .OrderByDescending(i => i.Id)
                          .ToListAsync();

        return _mapper.Map<IEnumerable<ItemViewModel>>(items);
    }

    public async Task<Result<ItemViewModel>> Create(ItemCreateViewModel createViewModel)
    {
        var entity = _mapper.Map<Items>(createViewModel);

        _dbContext.Add(entity);
        var writtenEntries = await _dbContext.SaveChangesAsync();

        return writtenEntries <= 0
            ? Result.Fail<ItemViewModel>("Could not create a new item.")
            : Result.Ok(_mapper.Map<ItemViewModel>(entity));
    }

    // TODO
    public async Task<Result<InventoryViewModel>> AddItemToCharacter(AddItemToCharacterViewModel addItemViewModel)
    {
        return default;
    }
}
