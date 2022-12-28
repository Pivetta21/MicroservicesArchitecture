using System.ComponentModel.DataAnnotations;

namespace Game.ViewModels;

public class DungeonEntranceCreateViewModel
{
    [Required]
    public required Guid CharacterTransactionId { get; set; }

    [Required]
    public required long DungeonId { get; set; }
}
