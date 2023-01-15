namespace Common.RabbitMq;

public class RabbitMqException : Exception
{
    public RabbitMqException()
    {
    }

    public RabbitMqException(string? message) : base(message)
    {
    }

    public RabbitMqException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
