using Game.Models;
using Game.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Game.Data;

public static class AppDbInitializer
{
    public static void SeedData(IApplicationBuilder app)
    {
        var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetService<GameDbContext>();

        if (context == null) return;

        try
        {
            if (context.Database.GetPendingMigrations().Any())
                context.Database.Migrate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Something went wrong while running migrations: {ex.Message}");
        }

        if (context.Dungeons.Any()) return;

        context.AddRange(SeedDungeons());
        context.SaveChanges();
    }

    private static IEnumerable<Dungeons> SeedDungeons()
    {
        var dungeons = new List<Dungeons>();

        var dungeon = new Dungeons
        {
            Name = "Ashes of Arcadia",
            TransactionId = Guid.NewGuid(),
            RequiredLevel = 1,
            Difficulty = 1,
            Cost = 5,
            MinExperience = 2,
            MaxExperience = 10,
            MinGold = 2,
            MaxGold = 10,
            Rewards = new List<Items>
            {
                new Armors
                {
                    TransactionId = Guid.NewGuid(),
                    Name = "Arcadia Shield",
                    Rarity = RarityEnum.Common,
                    MaxQuality = 3,
                    Resistance = 5,
                },
                new Weapons
                {
                    TransactionId = Guid.NewGuid(),
                    Name = "Arcadia Hammer",
                    Rarity = RarityEnum.Common,
                    MaxQuality = 3,
                    Power = 5,
                },
            },
        };
        dungeons.Add(dungeon);

        return dungeons;
    }
}
