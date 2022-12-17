using System.ComponentModel.DataAnnotations;
using Armory.Models.Enums;

namespace Armory.Models;

public class Characters
{
    public long Id { get; set; }

    public required Guid TransactionId { get; set; }

    public required Guid UserTransactionId { get; set; }

    [MaxLength(40)]
    public required string Name { get; set; }

    public required SpecializationEnum Specialization { get; set; }

    public int Life { get; set; } = 100;

    public int Damage { get; set; } = 5;

    public long Gold { get; set; } = 20;

    public int Level { get; set; } = 1;

    public double Experience { get; set; }

    public bool IsPlaying { get; set; }

    public long BuildId { get; set; }
    public required Builds Build { get; set; }

    public long InventoryId { get; set; }
    public required Inventories Inventory { get; set; }
}
