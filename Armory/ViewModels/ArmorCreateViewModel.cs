using System.ComponentModel.DataAnnotations;

namespace Armory.ViewModels;

public class ArmorCreateViewModel : ItemCreateViewModel
{
    [Required]
    public required int Resistance { get; set; }
}
