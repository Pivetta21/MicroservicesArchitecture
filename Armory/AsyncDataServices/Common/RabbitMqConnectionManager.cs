using RabbitMQ.Client;

namespace Armory.AsyncDataServices.Common;

public sealed class RabbitMqConnectionManager : IDisposable
{
    public IConnection ProducerConnection { get; }

    public IConnection ConsumerConnection { get; }

    public RabbitMqConnectionManager(IConnectionFactory connectionFactory)
    {
        ProducerConnection = connectionFactory.CreateConnection();
        ConsumerConnection = connectionFactory.CreateConnection();
    }

    public void Dispose()
    {
        ProducerConnection.Close();
        ProducerConnection.Dispose();

        ConsumerConnection.Close();
        ConsumerConnection.Dispose();

        Console.WriteLine("RabbitMQ connections disposed successfully");
    }
}
