using Common.RabbitMq.Enums;
using RabbitMQ.Client;

namespace Common.RabbitMq;

public abstract class RabbitMqProducerBase : RabbitMqClientBase
{
    protected RabbitMqProducerBase(
        IConnection connection,
        ExchangeTypeEnum exchangeType,
        ExchangesEnum exchange,
        QueuesEnum queue,
        string? routingKey = null
    ) : base(connection, exchangeType, exchange, queue, routingKey)
    {
    }

    protected void BasicPublish(
        byte[] body,
        IBasicProperties properties
    )
    {
        Channel.BasicPublish(
            exchange: _exchange,
            routingKey: _routingKey,
            body: new ReadOnlyMemory<byte>(body),
            basicProperties: properties
        );
    }

    protected IBasicProperties CreateBasicProperties(SagaInfo sagaInfo)
    {
        var properties = Channel!.CreateBasicProperties();

        properties.ContentType = "application/json";

        properties.Headers ??= new Dictionary<string, object>();
        properties.Headers.Add(SagaInfo.SagaNameKey, sagaInfo.SagaName);
        properties.Headers.Add(SagaInfo.CorrelationIdKey, sagaInfo.SagaCorrelationId);

        return properties;
    }
}
