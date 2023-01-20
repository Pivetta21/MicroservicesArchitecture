using FluentResults;
using Game.ViewModels;

namespace Game.Services.Interfaces;

public interface IDungeonJournalService
{
    Task<IEnumerable<DungeonJournalViewModel>> GetAll();

    Task<Result<DungeonJournalViewModel>> GetById(long id);
}
