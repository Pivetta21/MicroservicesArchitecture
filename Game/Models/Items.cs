using Game.Models.Enums;

namespace Game.Models;

public abstract class Items
{
    public long Id { get; set; }

    public required Guid TransactionId { get; set; } = Guid.NewGuid();

    public required string Name { get; set; }

    public required RarityEnum Rarity { get; set; }

    public required int MaxQuality { get; set; }

    public required long Price { get; set; }

    public ICollection<Dungeons> Dungeons { get; } = new HashSet<Dungeons>();

    public ICollection<DungeonJournals> DungeonJournals { get; } = new HashSet<DungeonJournals>();
}
