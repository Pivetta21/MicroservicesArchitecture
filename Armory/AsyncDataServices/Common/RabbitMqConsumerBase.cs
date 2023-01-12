using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Armory.AsyncDataServices.Common;

public abstract class RabbitMqConsumerBase : RabbitMqClientBase
{
    protected RabbitMqConsumerBase(
        ILogger<RabbitMqConsumerBase> logger,
        IConnection connection,
        string exchangeType,
        ExchangesEnum exchange,
        QueuesEnum queue,
        string? routingKey = null
    ) : base(logger, connection, exchangeType, exchange, queue, routingKey)
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
