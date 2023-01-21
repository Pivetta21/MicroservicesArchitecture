using Armory.Data;
using Armory.Models;
using Armory.Models.Enums;
using Armory.Services.Interfaces;
using Armory.ViewModels;
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Armory.Services;

public class CharacterService : ICharacterService
{
    private readonly IMapper _mapper;
    private readonly ArmoryDbContext _dbContext;

    public CharacterService(
        IMapper mapper,
        ArmoryDbContext dbContext
    )
    {
        _mapper = mapper;
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<CharacterViewModel>> GetAll()
    {
        var characters = await QueryEager()
                               .OrderByDescending(c => c.Id)
                               .ToListAsync();

        return _mapper.Map<IEnumerable<CharacterViewModel>>(characters);
    }

    public async Task<Result<CharacterViewModel>> GetByTransactionId(Guid transactionId)
    {
        var character = await QueryEager().FirstOrDefaultAsync(a => a.TransactionId == transactionId);

        return character == null
            ? Result.Fail<CharacterViewModel>($"Not found a character with transaction id equal to '{transactionId}'")
            : Result.Ok(_mapper.Map<CharacterViewModel>(character));
    }

    public async Task<Result<CharacterViewModel>> Create(CharacterCreateViewModel createViewModel)
    {
        var entity = new Characters
        {
            Name = createViewModel.Name,
            Specialization = createViewModel.Specialization,
            TransactionId = Guid.NewGuid(),
            UserTransactionId = Guid.NewGuid(),
            Build = new Builds
            {
                Armor = new Armors
                {
                    Name = "Basic Armor",
                    Rarity = RarityEnum.Common,
                    Resistance = 2,
                },
                Weapon = new Weapons
                {
                    Name = "Basic Weapon",
                    Rarity = RarityEnum.Common,
                    Power = 2,
                },
            },
            Inventory = new Inventories(),
        };

        _dbContext.Characters.Add(entity);
        var writtenEntries = await _dbContext.SaveChangesAsync();

        return writtenEntries <= 0
            ? Result.Fail<CharacterViewModel>("Could not create a new character.")
            : Result.Ok(_mapper.Map<CharacterViewModel>(entity));
    }

    public async Task<Result> Update(Guid transactionId, CharacterUpdateViewModel updateViewModel)
    {
        var entity = await _dbContext.Characters.FirstOrDefaultAsync(c => c.TransactionId == transactionId);

        if (entity == null)
            return Result.Fail($"Character '{transactionId}' not found.");

        _mapper.Map(updateViewModel, entity);
        var writtenEntries = await _dbContext.SaveChangesAsync();

        return writtenEntries > 0 ? Result.Ok() : Result.Fail($"Could not update character '{transactionId}'");
    }

    public async Task<Result> Delete(Guid transactionId)
    {
        var entity = await _dbContext.Characters.FirstOrDefaultAsync(c => c.TransactionId == transactionId);

        if (entity == null)
            return Result.Fail($"Character '{transactionId}' not found.");

        _dbContext.Remove(entity);
        var writtenEntries = await _dbContext.SaveChangesAsync();

        return writtenEntries > 0 ? Result.Ok() : Result.Fail($"Could not delete character '{transactionId}'");
    }

    public async Task<Result<InventoryViewModel>> AddRewardToInventory(AddRewardToCharacterViewModel addRewardViewModel)
    {
        var characterTransactionId = addRewardViewModel.CharacterTransactionId;
        var reward = addRewardViewModel.Reward;

        var character = await _dbContext
                              .Characters
                              .Include(c => c.Inventory.Items)
                              .Include(c => c.Build)
                              .FirstOrDefaultAsync(c => c.TransactionId == characterTransactionId);

        if (character == null)
            return Result.Fail<InventoryViewModel>($"Character {characterTransactionId} not found");

        if (character.Inventory.Items.Count >= 20)
            return Result.Fail<InventoryViewModel>($"Character {character.TransactionId} inventory is full");

        var item = _mapper.Map<Items>(reward);
        item.InventoryId = character.InventoryId;

        _dbContext.Items.Add(item);
        var writtenEntries = await _dbContext.SaveChangesAsync();

        return writtenEntries <= 0
            ? Result.Fail<InventoryViewModel>("Could not add a new reward to a character inventory.")
            : Result.Ok(_mapper.Map<InventoryViewModel>(character.Inventory));
    }

    private IQueryable<Characters> QueryEager()
    {
        return _dbContext.Characters
                         .Include(c => c.Inventory.Items)
                         .Include(c => c.Build.Armor)
                         .Include(c => c.Build.Weapon);
    }
}
