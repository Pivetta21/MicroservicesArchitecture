using System.ComponentModel.DataAnnotations;

namespace Armory.ViewModels;

public class WeaponCreateViewModel : ItemCreateViewModel
{
    [Required]
    public required int Power { get; set; }
}
