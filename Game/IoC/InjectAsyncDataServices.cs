using Game.AsyncDataServices;

namespace Game.IoC;

public static class InjectAsyncDataServices
{
    public static void AddAsyncDataServices(this IServiceCollection services)
    {
        services.AddSingleton<DungeonEntranceProducer>();
        services.AddHostedService<DungeonEntranceConsumer>();

        services.AddSingleton<PlayDungeonReplyProducer>();
        services.AddHostedService<PlayDungeonGameRequestConsumer>();
    }
}
