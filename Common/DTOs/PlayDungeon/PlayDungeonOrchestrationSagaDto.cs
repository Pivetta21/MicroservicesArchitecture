namespace Common.DTOs.PlayDungeon;

public class PlayDungeonOrchestrationSagaDto
{
    public string Message { get; set; } = null!;

    public Guid DungeonEntranceTransactionId { get; set; }
}
