using Armory.Services;
using Armory.Services.Interfaces;

namespace Armory.IoC;

public static class InjectServices
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<ICharacterService, CharacterService>();
        services.AddScoped<IDungeonEntranceService, DungeonEntranceService>();
    }
}
