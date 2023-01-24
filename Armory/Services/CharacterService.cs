using Armory.Data;
using Armory.Models;
using Armory.Models.Enums;
using Armory.Services.Interfaces;
using Armory.SyncDataServices;
using Armory.ViewModels;
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Armory.Services;

public class CharacterService : ICharacterService
{
    private readonly IMapper _mapper;
    private readonly ArmoryDbContext _dbContext;
    private readonly GameItemsHttpService _gameItemsHttpService;

    public CharacterService(
        IMapper mapper,
        ArmoryDbContext dbContext,
        GameItemsHttpService gameItemsHttpService
    )
    {
        _mapper = mapper;
        _dbContext = dbContext;
        _gameItemsHttpService = gameItemsHttpService;
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
            Inventory = new Inventories
            {
                Label = $"{createViewModel.Name}'s Inventory",
                CreatedAt = DateTime.UtcNow,
            },
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

    public async Task<Result<InventoryViewModel>> SellItem(Guid characterTransactionId, long itemId)
    {
        var character = await _dbContext.Characters
                                        .Include(c => c.Inventory.Items)
                                        .FirstOrDefaultAsync(c => c.TransactionId == characterTransactionId);

        if (character == null)
            return Result.Fail<InventoryViewModel>($"Character {characterTransactionId} not found");

        var item = await _dbContext.Items.FirstOrDefaultAsync(i => i.Id == itemId);

        if (item == null)
            return Result.Fail<InventoryViewModel>($"Item {itemId} not found");

        var isOnBuild = await _dbContext.Builds
                                        .Where(b => b.Id == character.BuildId)
                                        .AnyAsync(b => b.ArmorId == item.Id || b.WeaponId == item.Id);

        if (isOnBuild)
            return Result.Fail<InventoryViewModel>($"Item {item.Id} cant be sold because is allocated to your build");

        var isOnInventory = await _dbContext.Items
                                            .Where(i => i.InventoryId == character.InventoryId)
                                            .AnyAsync(i => i.Id == item.Id);

        if (!isOnInventory)
            return Result.Fail<InventoryViewModel>($"Item {item.Id} can only be sold if present in your inventory");

        var gameItem = await _gameItemsHttpService.GetItemAsync(item.TransactionId);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            character.Gold += gameItem?.Price ?? 0;
            _dbContext.Characters.Update(character);

            if (await _dbContext.SaveChangesAsync() <= 0)
                throw new Exception($"Could not update character {character.TransactionId} gold");

            character.Inventory.Items.Remove(item);
            _dbContext.Items.Remove(item);

            if (await _dbContext.SaveChangesAsync() <= 0)
                throw new Exception($"Could not remove item {item.Id} from character inventory");

            await transaction.CommitAsync();
            return Result.Ok(_mapper.Map<InventoryViewModel>(character.Inventory));
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return Result.Fail<InventoryViewModel>("Something unexpected happened and item could not be sold");
        }
    }

    public async Task<Result<BuildViewModel>> EquipItem(Guid characterTransactionId, long itemId)
    {
        var character = await _dbContext.Characters
                                        .Include(c => c.Build.Armor)
                                        .Include(c => c.Build.Weapon)
                                        .Include(c => c.Inventory.Items)
                                        .FirstOrDefaultAsync(c => c.TransactionId == characterTransactionId);

        if (character == null)
            return Result.Fail<BuildViewModel>($"Character {characterTransactionId} not found");

        var item = await _dbContext.Items.FirstOrDefaultAsync(i => i.Id == itemId);

        if (item == null)
            return Result.Fail<BuildViewModel>($"Item {itemId} not found");

        var isOnBuild = character.Build.WeaponId == item.Id || character.Build.ArmorId == item.Id;

        if (isOnBuild)
            return Result.Fail<BuildViewModel>($"Item {itemId} is already equipped");

        var isOnInventory = character.Inventory.Items.Any(i => i.Id == item.Id);

        if (!isOnInventory)
            return Result.Fail<BuildViewModel>($"Item {itemId} is not present in your inventory");

        switch (item)
        {
            case Weapons weapon:
                character.Inventory.Items.Add(character.Build.Weapon);
                character.Build.Weapon = weapon;
                break;
            case Armors armor:
                character.Inventory.Items.Add(character.Build.Armor);
                character.Build.Armor = armor;
                break;
            default:
                return Result.Fail<BuildViewModel>($"There is something wrong with item {itemId}");
        }

        character.Inventory.Items.Remove(item);

        var writtenEntries = await _dbContext.SaveChangesAsync();
        return writtenEntries <= 0
            ? Result.Fail<BuildViewModel>("Could not equip item to a character build.")
            : Result.Ok(_mapper.Map<BuildViewModel>(character.Build));
    }

    private IQueryable<Characters> QueryEager()
    {
        return _dbContext.Characters
                         .Include(c => c.Inventory.Items)
                         .Include(c => c.Build.Armor)
                         .Include(c => c.Build.Weapon);
    }
}
