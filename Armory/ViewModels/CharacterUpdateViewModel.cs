using System.ComponentModel.DataAnnotations;

namespace Armory.ViewModels;

public class CharacterUpdateViewModel
{
    [Required]
    [MaxLength(40)]
    public required string Name { get; set; }
}
