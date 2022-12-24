using Game.Models;
using Microsoft.EntityFrameworkCore;

namespace Game.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Dungeons> Dungeons => Set<Dungeons>();
    public DbSet<DungeonJournals> DungeonJournals => Set<DungeonJournals>();
    public DbSet<DungeonEntrances> DungeonEntrances => Set<DungeonEntrances>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Items>().ToTable("items");
        modelBuilder.Entity<Weapons>().ToTable("weapons");
        modelBuilder.Entity<Armors>().ToTable("armors");
    }
}
