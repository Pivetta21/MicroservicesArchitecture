using Common.RabbitMq.Enums;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Common.RabbitMq;

public abstract class RabbitMqConsumerBase : RabbitMqClientBase
{
    protected RabbitMqConsumerBase(
        IConnection connection,
        ExchangeTypeEnum exchangeType,
        ExchangesEnum exchange,
        QueuesEnum queue,
        string? routingKey = null
    ) : base(connection, exchangeType, exchange, queue, routingKey)
    {
        var consumer = new AsyncEventingBasicConsumer(Channel);

        consumer.Received += OnEventReceived;

        Channel.BasicConsume(
            queue: _queue,
            autoAck: false,
            consumer: consumer
        );
    }

    protected abstract Task OnEventReceived(object? sender, BasicDeliverEventArgs @event);
}
