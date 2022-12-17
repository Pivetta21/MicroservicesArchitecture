namespace Armory.Models;

public class Armors : Items
{
    public required int Resistance { get; set; }

    public Builds Build { get; set; } = null!;
}
