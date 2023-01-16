namespace Game.ViewModels;

public class DungeonEntranceViewModel
{
    public long Id { get; set; }

    public Guid CharacterTransactionId { get; set; }

    public bool Processed { get; set; }

    public long Cost { get; set; }

    public DungeonViewModel Dungeon { get; set; } = null!;
}
