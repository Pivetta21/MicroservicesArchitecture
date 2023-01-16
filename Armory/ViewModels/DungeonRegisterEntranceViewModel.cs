using System.ComponentModel.DataAnnotations;

namespace Armory.ViewModels;

public class DungeonRegisterEntranceViewModel
{
    [Required]
    public Guid CharacterTransactionId { get; set; }

    [Required]
    public Guid DungeonTransactionId { get; set; }
}
