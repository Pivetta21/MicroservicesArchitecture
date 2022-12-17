using Armory.Models.Enums;

namespace Armory.ViewModels;

public class CharacterViewModel
{
    public Guid TransactionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Level { get; set; }

    public long Gold { get; set; }

    public bool IsPLaying { get; set; }

    public SpecializationEnum Specialization { get; set; }

    public string SpecializationDescription { get; set; } = string.Empty;

    public BuildViewModel Build { get; set; } = null!;

    public InventoryViewModel Inventory { get; set; } = null!;
}
