namespace Common.DTOs.DungeonEntrance;

public class DungeonEntranceFeeException : Exception
{
    public DungeonEntranceFeeException()
    {
    }

    public DungeonEntranceFeeException(string? message) : base(message)
    {
    }

    public DungeonEntranceFeeException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
