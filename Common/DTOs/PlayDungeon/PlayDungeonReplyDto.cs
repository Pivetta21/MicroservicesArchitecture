using Common.DTOs.Item;
using Common.RabbitMq;

namespace Common.DTOs.PlayDungeon;

public class PlayDungeonReplyDto : SagaInfo
{
    public required PlayDungeonEventEnum PlayDungeonEvent { get; set; }

    public required Guid DungeonEntranceTransactionId { get; set; }

    public ItemRewardDto? EarnedItem { get; set; }

    public int? EarnedExperience { get; set; }

    public int? EarnedGold { get; set; }
}
