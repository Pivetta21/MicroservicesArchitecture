using Armory.Models;
using Microsoft.EntityFrameworkCore;

namespace Armory.Data;

public class ArmoryDbContext : DbContext
{
    public ArmoryDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Characters> Characters => Set<Characters>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Items>().ToTable("items");
        modelBuilder.Entity<Armors>().ToTable("armors");
        modelBuilder.Entity<Weapons>().ToTable("weapons");
    }
}
