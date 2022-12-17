namespace Armory.Models;

public class Builds
{
    public long Id { get; set; }

    public Characters Character { get; set; } = null!;

    public long WeaponId { get; set; }
    public required Weapons Weapon { get; set; }

    public long ArmorId { get; set; }
    public required Armors Armor { get; set; }
}
