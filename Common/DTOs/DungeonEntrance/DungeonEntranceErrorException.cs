namespace Common.DTOs.DungeonEntrance;

public class DungeonEntranceErrorException : Exception
{
    public DungeonEntranceErrorException()
    {
    }

    public DungeonEntranceErrorException(string? message) : base(message)
    {
    }

    public DungeonEntranceErrorException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
