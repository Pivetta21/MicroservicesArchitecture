using Game.Services;

namespace Game.IoC;

public static class InjectServices
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IDungeonService, DungeonService>();

        services.AddSingleton<IProofOfWork, ProofOfWork>();
    }
}
