using Microsoft.EntityFrameworkCore;

namespace Armory.Data;

public static class AppDbInitializer
{
    public static void SeedData(IApplicationBuilder app)
    {
        var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetService<ArmoryDbContext>();

        if (context is null) return;

        try
        {
            if (context.Database.GetPendingMigrations().Any())
                context.Database.Migrate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not run migrations: {ex.Message}");
        }
    }
}
