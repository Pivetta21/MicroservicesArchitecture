namespace Common.DTOs.DungeonEntrance;

public class DungeonEntranceArmoryDto
{
    public required DungeonEntranceEventEnum DungeonEntranceEvent { get; set; }

    public required Guid DungeonEntranceTransactionId { get; set; }

    public Guid? DungeonTransactionId { get; set; }

    public Guid? CharacterTransactionId { get; set; }
}
