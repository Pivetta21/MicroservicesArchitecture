using RabbitMQ.Client;

namespace Common.RabbitMq;

public interface IRabbitMqProducer<in T>
{
    void Publish(T @event, SagaInfo sagaInfo);

    void Publish(T @event, IBasicProperties props);
}
