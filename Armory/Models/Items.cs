using System.ComponentModel.DataAnnotations;
using Armory.Models.Enums;

namespace Armory.Models;

public abstract class Items
{
    public long Id { get; set; }

    public Guid TransactionId { get; set; } = Guid.NewGuid();

    [MaxLength(40)]
    public required string Name { get; set; }

    public required RarityEnum Rarity { get; set; }

    public long? InventoryId { get; set; }
    public Inventories? Inventory { get; set; }
}
