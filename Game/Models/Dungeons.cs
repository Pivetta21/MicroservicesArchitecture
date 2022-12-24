namespace Game.Models;

public class Dungeons
{
    public long Id { get; set; }

    public required string Name { get; set; }

    public required int RequiredLevel { get; set; }

    public required int Difficulty { get; set; }

    public required long Cost { get; set; }

    public required long MinExperience { get; set; }

    public required long MaxExperience { get; set; }

    public required long MinGold { get; set; }

    public required long MaxGold { get; set; }

    public required ICollection<Items> Rewards { get; set; } = new HashSet<Items>();
}
