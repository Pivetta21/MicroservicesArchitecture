namespace Armory.Models;

public class Weapons : Items
{
    public required int Power { get; set; }

    public Builds Build { get; set; } = null!;
}
