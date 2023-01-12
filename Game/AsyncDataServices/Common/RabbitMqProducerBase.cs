using RabbitMQ.Client;

namespace Game.AsyncDataServices.Common;

public abstract class RabbitMqProducerBase : RabbitMqClientBase
{
    protected RabbitMqProducerBase(
        ILogger<RabbitMqProducerBase> logger,
        IConnection connection,
        string exchangeType,
        ExchangesEnum exchange,
        QueuesEnum queue,
        string? routingKey = null
    ) : base(logger, connection, exchangeType, exchange, queue, routingKey)
    {
    }

    protected void BasicPublish(
        byte[] body,
        IBasicProperties? properties = null
    )
    {
        if (Channel?.IsClosed ?? true)
        {
            _logger.LogError("Could not publish because the channel is closed");
            return;
        }

        if (properties == null)
        {
            properties = Channel.CreateBasicProperties();
            properties.AppId = AppDomain.CurrentDomain.FriendlyName;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.CorrelationId = Guid.NewGuid().ToString();
            properties.ContentType = "application/json";
        }

        Channel.BasicPublish(
            exchange: _exchange,
            routingKey: _routingKey,
            body: new ReadOnlyMemory<byte>(body),
            basicProperties: properties
        );
    }
}
