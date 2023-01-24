using Common.DTOs.Item;

namespace Common.DTOs.PlayDungeon;

public class PlayDungeonReplyDto
{
    public required PlayDungeonEventEnum PlayDungeonEvent { get; set; }

    public required Guid DungeonEntranceTransactionId { get; set; }

    public ItemRewardDto? EarnedItem { get; set; }

    public int? EarnedExperience { get; set; }

    public int? EarnedGold { get; set; }
}
