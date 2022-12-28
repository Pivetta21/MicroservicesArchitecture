using Game.Models.Enums;

namespace Game.ViewModels;

public class DungeonEntranceViewModel
{
    public long Id { get; set; }

    public DungeonEntranceStatusEnum Status { get; set; }

    public string StatusDescription { get; set; } = string.Empty;

    public bool Processed { get; set; }

    public Guid CharacterTransactionId { get; set; }

    public DungeonViewModel Dungeon { get; set; } = null!;
}
