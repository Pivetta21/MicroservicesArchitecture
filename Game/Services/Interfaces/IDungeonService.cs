using Common.DTOs.PlayDungeon;
using FluentResults;
using Game.ViewModels;

namespace Game.Services.Interfaces;

public interface IDungeonService
{
    Task<IEnumerable<DungeonViewModel>> GetAll();

    Task<Result<DungeonViewModel>> GetByTransactionId(Guid transactionId);

    Task ProcessPlayDungeonGameRequest(PlayDungeonGameDto dto);
}
