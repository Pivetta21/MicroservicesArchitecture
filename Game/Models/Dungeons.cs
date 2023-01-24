namespace Game.Models;

public class Dungeons
{
    public long Id { get; set; }

    public required Guid TransactionId { get; set; } = Guid.NewGuid();

    public required string Name { get; set; }

    public required int RequiredLevel { get; set; }

    public required int Difficulty { get; set; }

    public required long Cost { get; set; }

    public required int MinExperience { get; set; }

    public required int MaxExperience { get; set; }

    public required int MinGold { get; set; }

    public required int MaxGold { get; set; }

    public required ICollection<Items> Rewards { get; set; } = new HashSet<Items>();
}
