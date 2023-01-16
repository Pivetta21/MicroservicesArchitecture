using Armory.Models.Enums;

namespace Armory.ViewModels;

public class DungeonEntranceViewModel
{
    public long Id { get; set; }

    public Guid DungeonTransactionId { get; set; }

    public long? Fee { get; set; }

    public DungeonEntranceStatusEnum Status { get; set; }

    public string StatusDescription { get; set; } = string.Empty;
}
