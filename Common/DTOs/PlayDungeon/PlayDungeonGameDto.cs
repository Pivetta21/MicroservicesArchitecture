namespace Common.DTOs.PlayDungeon;

public class PlayDungeonGameDto
{
    public required PlayDungeonEventEnum PlayDungeonEvent { get; set; }

    public required Guid DungeonEntranceTransactionId { get; set; }
}
