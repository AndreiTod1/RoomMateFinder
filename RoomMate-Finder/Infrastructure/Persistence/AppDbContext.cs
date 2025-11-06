using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;

namespace RoomMate_Finder.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Profile> Profiles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Profile>().ToTable("profiles", "public");
        base.OnModelCreating(modelBuilder);
    }
}