using Common.RabbitMq.Enums;
using RabbitMQ.Client;

namespace Common.RabbitMq;

public abstract class RabbitMqClientBase : IDisposable
{
    private readonly IConnection _connection;

    protected IModel? Channel { get; private set; }

    private readonly string _exchangeType;
    protected readonly string _exchange;
    protected readonly string _queue;
    protected readonly string _routingKey;

    protected RabbitMqClientBase(
        IConnection connection,
        ExchangeTypeEnum exchangeType,
        ExchangesEnum exchange,
        QueuesEnum queue,
        string? routingKey = null
    )
    {
        // Use a long-lived connection
        _connection = connection;

        _exchangeType = exchangeType switch
        {
            ExchangeTypeEnum.Direct => ExchangeType.Direct,
            ExchangeTypeEnum.Fanout => ExchangeType.Fanout,
            ExchangeTypeEnum.Headers => ExchangeType.Headers,
            _ => ExchangeType.Direct,
        };

        _exchange = exchange.ToString();

        _queue = queue.ToString();
        _routingKey = routingKey ?? _queue;

        ConnectToRabbitMq();
    }

    protected void CreateChannel()
    {
        // Use one channel per "task"
        if (Channel == null || Channel.IsClosed)
            Channel = _connection.CreateModel();
    }

    private void ConnectToRabbitMq()
    {
        if (!_connection.IsOpen)
        {
            Console.WriteLine("Could not create a RabbitMQ client because the connection is closed");
            return;
        }

        CreateChannel();

        Channel.ExchangeDeclare(
            type: _exchangeType,
            exchange: _exchange,
            durable: true,
            autoDelete: false
        );

        // A single queue for 'n' consumers (i. e., exactly once semantics)
        Channel.QueueDeclare(
            queue: _queue,
            durable: false,
            exclusive: false,
            autoDelete: false
        );

        Channel.QueueBind(
            queue: _queue,
            exchange: _exchange,
            routingKey: _routingKey
        );
    }

    public void Dispose()
    {
        try
        {
            if (Channel is { IsOpen: true })
            {
                Channel.Close();
                Channel.Dispose();
                Channel = null;
            }

            Console.WriteLine("RabbitMQ client disposed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Something went wrong while disposing the RabbitMQ Client: {ex.Message}");
        }

        GC.SuppressFinalize(this);
    }
}
