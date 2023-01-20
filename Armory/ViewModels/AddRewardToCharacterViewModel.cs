using System.ComponentModel.DataAnnotations;

namespace Armory.ViewModels;

public class AddRewardToCharacterViewModel
{
    [Required]
    public required ItemCreateViewModel Reward { get; set; }

    [Required]
    public required Guid CharacterTransactionId { get; set; }
}
