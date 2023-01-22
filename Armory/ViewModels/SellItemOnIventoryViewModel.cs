using System.ComponentModel.DataAnnotations;

namespace Armory.ViewModels;

public class SellItemOnInventoryViewModel
{
    [Required]
    public required long ItemId { get; set; }
}
