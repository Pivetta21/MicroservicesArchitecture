using Armory.Models.Enums;

namespace Armory.ViewModels;

public abstract class ItemViewModel
{
    public Guid TransactionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public RarityEnum Rarity { get; set; }

    public string RarityDescription { get; set; } = string.Empty;
}
