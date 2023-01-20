using System.ComponentModel.DataAnnotations;

namespace Armory.ViewModels;

public class AddItemToCharacterViewModel
{
    [Required]
    public required long ItemId { get; set; }

    [Required]
    public required Guid CharacterTransactionId { get; set; }
}
