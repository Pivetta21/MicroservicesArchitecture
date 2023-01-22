using Armory.SyncDataServices;

namespace Armory.IoC;

public static class InjectSyncDataServices
{
    public static void AddSyncDataServices(this IServiceCollection services)
    {
        services.AddHttpClient<GameItemsHttpService>();
    }
}
