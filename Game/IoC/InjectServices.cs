using Game.Services;
using Game.Services.Interfaces;

namespace Game.IoC;

public static class InjectServices
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IDungeonService, DungeonService>();
        services.AddScoped<IDungeonEntranceService, DungeonEntranceService>();

        services.AddSingleton<IProofOfWork, ProofOfWork>();
    }
}
