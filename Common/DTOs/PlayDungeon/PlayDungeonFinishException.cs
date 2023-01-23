namespace Common.DTOs.PlayDungeon;

public class PlayDungeonFinishException : Exception
{
    public PlayDungeonFinishException()
    {
    }

    public PlayDungeonFinishException(string? message) : base(message)
    {
    }

    public PlayDungeonFinishException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
