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

    public async Task<Result<InventoryViewModel>> AddItemToCharacter(AddItemToCharacterViewModel addItemViewModel)
    {
        var item = await _dbContext
                         .Items
                         .FirstOrDefaultAsync(i => i.Id == addItemViewModel.ItemId);

        if (item == null)
            return Result.Fail<InventoryViewModel>($"Item {addItemViewModel.ItemId} not found");

        if (item.InventoryId != null)
            return Result.Fail<InventoryViewModel>($"Item {item.Id} is already allocated to an inventory");

        var character = await _dbContext
                              .Characters
                              .Include(c => c.Inventory.Items)
                              .Include(c => c.Build)
                              .FirstOrDefaultAsync(c => c.TransactionId == addItemViewModel.CharacterTransactionId);

        if (character == null)
            return Result.Fail<InventoryViewModel>($"Character {addItemViewModel.CharacterTransactionId} not found");
        
        if (character.Inventory.Items.Count >= 20)
            return Result.Fail<InventoryViewModel>($"Character {character.TransactionId} inventory is full");

        if (character.Build.ArmorId == item.Id || character.Build.WeaponId == item.Id)
            return Result.Fail<InventoryViewModel>($"Item {item.Id} os already allocated to an build slot");

        item.InventoryId = character.InventoryId;

        _dbContext.Items.Update(item);
        var writtenEntries = await _dbContext.SaveChangesAsync();

        return writtenEntries <= 0
            ? Result.Fail<InventoryViewModel>("Could not add a new item to character.")
            : Result.Ok(_mapper.Map<InventoryViewModel>(character.Inventory));
    }
}
