using Armory.Models.Enums;

namespace Armory.Models;

public class DungeonEntrances
{
    public long Id { get; set; }

    public required Guid TransactionId { get; set; }

    public required Guid DungeonTransactionId { get; set; }

    public long? PayedFee { get; set; }

    public required DungeonEntranceStatusEnum Status { get; set; }

    public required bool Deleted { get; set; }

    public long CharacterId { get; set; }
    public required Characters Character { get; set; }
}
