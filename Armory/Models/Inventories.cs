namespace Armory.Models;

public class Inventories
{
    public long Id { get; set; }

    public required string Label { get; set; }

    public required DateTime CreatedAt { get; set; }

    public Characters Character { get; set; } = null!;

    public ICollection<Items> Items { get; } = new HashSet<Items>();
}
