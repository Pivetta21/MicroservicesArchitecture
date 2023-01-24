using Armory.ViewModels;
using Common.DTOs.PlayDungeon;
using FluentResults;

namespace Armory.Services.Interfaces;

public interface IDungeonService
{
    Task<Result<DungeonEntranceViewModel>> PlayDungeon(PlayDungeonViewModel playDungeonViewModel);

    Task ProcessPlayDungeonReply(PlayDungeonReplyDto playDungeonReplyDto);
}
