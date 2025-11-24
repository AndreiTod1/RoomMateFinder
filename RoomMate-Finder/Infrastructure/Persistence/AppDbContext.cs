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
    public DbSet<Conversation> Conversations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Profile>().ToTable("profiles", "public");
        
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.ToTable("conversations", "public");
            
            entity.HasOne(c => c.User1)
                .WithMany()
                .HasForeignKey(c => c.User1Id)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(c => c.User2)
                .WithMany()
                .HasForeignKey(c => c.User2Id)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Ensure unique conversation between two users
            entity.HasIndex(c => new { c.User1Id, c.User2Id }).IsUnique();
        });
        
        base.OnModelCreating(modelBuilder);
    }
}