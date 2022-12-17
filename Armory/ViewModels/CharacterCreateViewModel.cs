using System.ComponentModel.DataAnnotations;
using Armory.Models.Enums;

namespace Armory.ViewModels;

public class CharacterCreateViewModel : CharacterUpdateViewModel
{
    [Required]
    public required SpecializationEnum Specialization { get; set; }
}
