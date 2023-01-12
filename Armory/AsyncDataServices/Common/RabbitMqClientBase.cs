using RabbitMQ.Client;

namespace Armory.AsyncDataServices.Common;

public abstract class RabbitMqClientBase : IDisposable
{
    protected readonly ILogger<RabbitMqClientBase> _logger;

    private readonly IConnection _connection;

    protected IModel? Channel { get; private set; }

    private readonly string _exchangeType;
    protected readonly string _exchange;
    protected readonly string _queue;
    protected readonly string _routingKey;

    protected RabbitMqClientBase(
        ILogger<RabbitMqClientBase> logger,
        IConnection connection,
        string exchangeType,
        ExchangesEnum exchange,
        QueuesEnum queue,
        string? routingKey = null
    )
    {
        _logger = logger;

        // Use a long-lived connection
        _connection = connection;

        _exchangeType = exchangeType;
        _queue = queue.ToString();
        _exchange = exchange.ToString();
        _routingKey = routingKey ?? _queue;

        ConnectToRabbitMq();
    }

    private void ConnectToRabbitMq()
    {
        if (_connection is { IsOpen: false })
            return;

        if (Channel is { IsOpen: false })
            return;

        // Use one channel per "task"
        Channel = _connection.CreateModel();

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

            _logger.LogInformation("RabbitMQ client disposed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Something went wrong while trying to dispose the RabbitMQ Client");
        }

        GC.SuppressFinalize(this);
    }
}
