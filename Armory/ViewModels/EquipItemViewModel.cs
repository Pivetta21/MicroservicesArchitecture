using System.ComponentModel.DataAnnotations;

namespace Armory.ViewModels;

public class EquipItemViewModel
{
    [Required]
    public required long ItemId { get; set; }
}
