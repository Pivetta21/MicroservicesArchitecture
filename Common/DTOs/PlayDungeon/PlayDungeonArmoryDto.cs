namespace Common.DTOs.PlayDungeon;

public class PlayDungeonArmoryDto
{
    public required PlayDungeonEventEnum PlayDungeonEvent { get; set; }

    public required Guid DungeonEntranceTransactionId { get; set; }
}
