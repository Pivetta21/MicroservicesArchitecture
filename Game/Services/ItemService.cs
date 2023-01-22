using AutoMapper;
using Common.DTOs.Item;
using FluentResults;
using Game.Data;
using Game.Services.Interfaces;
using Game.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Game.Services;

public class ItemService : IItemService
{
    private readonly ILogger<ItemService> _logger;
    private readonly IMapper _mapper;
    private readonly GameDbContext _dbContext;

    public ItemService(
        GameDbContext dbContext,
        IMapper mapper,
        ILogger<ItemService> logger
    )
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ItemViewModel>> GetAll()
    {
        var dungeons = await _dbContext.Items
                                       .OrderByDescending(d => d.Id)
                                       .ToListAsync();

        return _mapper.Map<IEnumerable<ItemViewModel>>(dungeons);
    }

    public async Task<Result<ItemViewModel>> GetByTransactionId(Guid transactionId)
    {
        var dungeon = await _dbContext.Items.FirstOrDefaultAsync(a => a.TransactionId == transactionId);

        return dungeon == null
            ? Result.Fail<ItemViewModel>($"An item with uuid equal to '{transactionId}' could not be found")
            : Result.Ok(_mapper.Map<ItemViewModel>(dungeon));
    }

    public async Task<Result<ItemPriceDto>> GetPriceByTransactionId(Guid transactionId)
    {
        _logger.LogInformation(
            "Searching for item {ItemTransactionId} on {ServiceName}",
            transactionId,
            AppDomain.CurrentDomain.FriendlyName
        );

        var item = await _dbContext.Items.FirstOrDefaultAsync(a => a.TransactionId == transactionId);

        if (item != null)
        {
            _logger.LogInformation(
                "Returning information about item {ItemTransactionId}",
                transactionId
            );

            return Result.Ok(_mapper.Map<ItemPriceDto>(item));
        }

        _logger.LogInformation(
            "Could not find any information about item {ItemTransactionId}",
            transactionId
        );

        return Result.Fail<ItemPriceDto>($"An item with uuid equal to '{transactionId}' could not be found");
    }
}
