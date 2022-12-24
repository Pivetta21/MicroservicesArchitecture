using System.ComponentModel.DataAnnotations;

namespace Game.ViewModels;

public class ArmorCreateViewModel : ItemCreateViewModel
{
    [Required]
    public required int Resistance { get; set; }
}
