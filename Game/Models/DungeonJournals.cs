namespace Game.Models;

public class DungeonJournals
{
    public long Id { get; set; }

    public required bool? WasSuccessful { get; set; }

    public required long ElapsedMilliseconds { get; set; }

    public required Guid CharacterTransactionId { get; set; }

    public required Guid DungeonEntranceTransactionId { get; set; }

    public long DungeonId { get; set; }
    public required Dungeons Dungeon { get; set; }

    public long? RewardId { get; set; }
    public required Items? Reward { get; set; }
}
