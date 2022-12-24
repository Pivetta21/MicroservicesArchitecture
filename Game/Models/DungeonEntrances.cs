using Game.Models.Enums;

namespace Game.Models;

public class DungeonEntrances
{
    public long Id { get; set; }

    public required DungeonEntranceStatusEnum Status { get; set; }

    public required bool Processed { get; set; }

    public required Guid CharacterTransactionId { get; set; }

    public long DungeonId { get; set; }
    public required Dungeons Dungeon { get; set; }
}
