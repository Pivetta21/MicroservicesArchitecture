using System.ComponentModel.DataAnnotations;

namespace Armory.ViewModels;

public class CharacterActionViewModel
{
    [Required]
    public required long ItemId { get; set; }
}
