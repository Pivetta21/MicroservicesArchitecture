namespace Game.ViewModels;

public class DungeonJournalViewModel
{
    public long Id { get; set; }

    public long DungeonId { get; set; }

    public Guid CharacterTransactionId { get; set; }

    public Guid DungeonEntranceTransactionId { get; set; }

    public bool WasSuccessful { get; set; }

    public long ElapsedMilliseconds { get; set; }

    public ItemViewModel Reward { get; set; } = null!;
}
