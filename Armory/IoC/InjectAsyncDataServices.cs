using Armory.AsyncDataServices;

namespace Armory.IoC;

public static class InjectAsyncDataServices
{
    public static void AddAsyncDataServices(this IServiceCollection services)
    {
        services.AddSingleton<DungeonEntranceProducer>();
        services.AddHostedService<DungeonEntranceConsumer>();

        services.AddSingleton<PlayDungeonGameRequestProducer>();

        services.AddSingleton<PlayDungeonReplyProducer>();
        services.AddHostedService<PlayDungeonReplyConsumer>();
    }
}
