using Common.RabbitMq;

namespace Common.DTOs.DungeonEntrance;

public class DungeonEntranceGameDto : SagaInfo
{
    public required DungeonEntranceEventEnum DungeonEntranceEvent { get; set; }

    public required Guid DungeonEntranceTransactionId { get; set; }

    public long? DungeonCost { get; set; }
}
