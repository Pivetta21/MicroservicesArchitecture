namespace Armory.AsyncDataServices.Common;

public interface IRabbitMqProducer<in T>
{
    void Publish(T @event);
}
