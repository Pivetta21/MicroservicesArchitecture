using System.ComponentModel.DataAnnotations;

namespace Armory.ViewModels;

public class PlayDungeonViewModel
{
    [Required]
    public Guid CharacterTransactionId { get; set; }

    [Required]
    public Guid DungeonEntranceTransactionId { get; set; }
}
