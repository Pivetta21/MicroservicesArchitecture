namespace Common.RabbitMq;

public interface IRabbitMqProducer<in T>
{
    void Publish(T @event);
}
