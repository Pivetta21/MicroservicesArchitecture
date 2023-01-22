using Armory.Models;
using Armory.Models.Enums;
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
        catch (Exception e)
        {
            Console.WriteLine($"Could not run migrations: {e.Message}");
        }

        if (context.Characters.Any()) return;

        context.AddRange(SeedCharacters());
        context.SaveChanges();
    }

    private static IEnumerable<Characters> SeedCharacters()
    {
        return new List<Characters>
        {
            new()
            {
                Name = "Bot Mage #001",
                TransactionId = Guid.NewGuid(),
                UserTransactionId = Guid.Empty,
                Specialization = SpecializationEnum.Mage,
                Build = new Builds
                {
                    Armor = new Armors
                    {
                        Name = "Basic Armor",
                        Rarity = RarityEnum.Common,
                        Resistance = 2,
                    },
                    Weapon = new Weapons
                    {
                        Name = "Basic Weapon",
                        Rarity = RarityEnum.Common,
                        Power = 2,
                    },
                },
                Inventory = new Inventories(),
            },
            new()
            {
                Name = "Bot Warrior #002",
                TransactionId = Guid.NewGuid(),
                UserTransactionId = Guid.Empty,
                Specialization = SpecializationEnum.Warrior,
                Build = new Builds
                {
                    Armor = new Armors
                    {
                        Name = "Basic Armor",
                        Rarity = RarityEnum.Common,
                        Resistance = 2,
                    },
                    Weapon = new Weapons
                    {
                        Name = "Basic Weapon",
                        Rarity = RarityEnum.Common,
                        Power = 2,
                    },
                },
                Inventory = new Inventories(),
            },
            new()
            {
                Name = "Bot Priest #003",
                TransactionId = Guid.NewGuid(),
                UserTransactionId = Guid.Empty,
                Specialization = SpecializationEnum.Priest,
                Build = new Builds
                {
                    Armor = new Armors
                    {
                        Name = "Basic Armor",
                        Rarity = RarityEnum.Common,
                        Resistance = 2,
                    },
                    Weapon = new Weapons
                    {
                        Name = "Basic Weapon",
                        Rarity = RarityEnum.Common,
                        Power = 2,
                    },
                },
                Inventory = new Inventories(),
            },
        };
    }
}
