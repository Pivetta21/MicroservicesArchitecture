using Common.RabbitMq;

namespace Common.DTOs.PlayDungeon;

public class PlayDungeonGameDto : SagaInfo
{
    public required PlayDungeonEventEnum PlayDungeonEvent { get; set; }

    public required Guid DungeonEntranceTransactionId { get; set; }
}
