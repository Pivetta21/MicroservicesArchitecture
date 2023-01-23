using Common.DTOs.Item;

namespace Common.DTOs.PlayDungeon;

public class PlayDungeonReplyDto
{
    public required PlayDungeonEventEnum PlayDungeonEvent { get; set; }

    public required Guid DungeonEntranceTransactionId { get; set; }

    public ItemRewardDto? ItemReward { get; set; }
}
