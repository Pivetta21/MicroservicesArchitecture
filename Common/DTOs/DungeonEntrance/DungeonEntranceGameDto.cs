namespace Common.DTOs.DungeonEntrance;

public class DungeonEntranceGameDto
{
    public required DungeonEntranceEventEnum DungeonEntranceEvent { get; set; }

    public required Guid DungeonEntranceTransactionId { get; set; }

    public long? DungeonCost { get; set; }
}
