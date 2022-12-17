namespace Armory.Models;

public class Inventories
{
    public long Id { get; set; }

    public int Size { get; set; } = 20;

    public Characters Character { get; set; } = null!;

    public ICollection<Items> Items { get; } = new HashSet<Items>();
}
