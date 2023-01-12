namespace Game.AsyncDataServices.Common;

public interface IRabbitMqProducer<in T>
{
    void Publish(T @event);
}
