namespace Armory.ViewModels;

public class InventoryViewModel
{
    public int Size { get; set; }

    public IEnumerable<ItemViewModel> Items { get; } = new List<ItemViewModel>();
}
