using Armory.AsyncDataServices;

namespace Armory.IoC;

public static class InjectAsyncDataServices
{
    public static void AddAsyncDataServices(this IServiceCollection services)
    {
        services.AddSingleton<DungeonEntranceProducer>();
        services.AddHostedService<DungeonEntranceConsumer>();

        services.AddSingleton<PlayDungeonGameRequestProducer>();
        services.AddHostedService<PlayDungeonReplyConsumer>();

        services.AddSingleton<PlayDungeonReplyProducer>();

        services.AddSingleton<PlayDungeonArmoryRequestProducer>();
    }
}
