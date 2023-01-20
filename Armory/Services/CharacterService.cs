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
        var characters = await QueryEager().ToListAsync();
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
            UserTransactionId = Guid.Empty,
            Build = new Builds
            {
                Armor = new Armors
                {
                    TransactionId = Guid.NewGuid(),
                    Name = "Basic Armor",
                    Rarity = RarityEnum.Common,
                    Resistance = 2,
                },
                Weapon = new Weapons
                {
                    TransactionId = Guid.NewGuid(),
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

    private IQueryable<Characters> QueryEager()
    {
        return _dbContext.Characters
                         .Include(c => c.Inventory)
                         .Include(c => c.Build.Armor)
                         .Include(c => c.Build.Weapon);
    }
}
