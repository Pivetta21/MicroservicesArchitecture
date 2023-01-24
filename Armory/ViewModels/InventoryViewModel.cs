namespace Armory.ViewModels;

public class InventoryViewModel
{
    public string Label { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public IEnumerable<ItemViewModel> Items { get; } = new List<ItemViewModel>();
}
