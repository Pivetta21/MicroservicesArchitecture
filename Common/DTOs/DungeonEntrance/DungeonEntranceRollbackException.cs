namespace Common.DTOs.DungeonEntrance;

public class DungeonEntranceRollbackException : Exception
{
    public DungeonEntranceRollbackException()
    {
    }

    public DungeonEntranceRollbackException(string? message) : base(message)
    {
    }

    public DungeonEntranceRollbackException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
