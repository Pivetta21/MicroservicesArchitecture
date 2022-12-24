using System.ComponentModel.DataAnnotations;

namespace Game.ViewModels;

public class DungeonCreateViewModel
{
    [Required]
    public required string Name { get; set; }

    [Required]
    public required int RequiredLevel { get; set; }

    [Required]
    public required int Difficulty { get; set; }

    [Required]
    public required long MinExperience { get; set; }

    [Required]
    public required long MaxExperience { get; set; }

    [Required]
    public required long MinGold { get; set; }

    [Required]
    public required long MaxGold { get; set; }

    [Required]
    public required long Cost { get; set; }

    [Required]
    public required IList<ItemCreateViewModel> Rewards { get; set; }
}
