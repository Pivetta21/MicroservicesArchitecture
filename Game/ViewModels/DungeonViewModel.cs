namespace Game.ViewModels;

public class DungeonViewModel
{
    public long Id { get; set; }

    public Guid TransactionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int RequiredLevel { get; set; }

    public int Difficulty { get; set; }

    public long Cost { get; set; }

    public long MinExperience { get; set; }

    public long MaxExperience { get; set; }

    public long MinGold { get; set; }

    public long MaxGold { get; set; }

    public IEnumerable<ItemViewModel> Rewards { get; set; } = new List<ItemViewModel>();
}
