using System.ComponentModel.DataAnnotations;

namespace Game.ViewModels;

public class WeaponCreateViewModel : ItemCreateViewModel
{
    [Required]
    public required int Power { get; set; }
}
