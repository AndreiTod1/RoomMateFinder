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
    public DbSet<UserAction> UserActions { get; set; } = null!;
    public DbSet<Match> Matches { get; set; } = null!;
    public DbSet<Conversation> Conversations { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Profile>().ToTable("profiles", "public");
        
        modelBuilder.Entity<UserAction>(entity =>
        {
            entity.ToTable("user_actions", "public");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ActionType).HasConversion<int>();
            
            // Configure relationships
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.TargetUser)
                  .WithMany()
                  .HasForeignKey(e => e.TargetUserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            // Prevent duplicate actions from same user to same target
            entity.HasIndex(e => new { e.UserId, e.TargetUserId })
                  .IsUnique();
        });
        
        modelBuilder.Entity<Match>(entity =>
        {
            entity.ToTable("matches", "public");
            entity.HasKey(e => e.Id);
            
            // Configure relationships
            entity.HasOne(e => e.User1)
                  .WithMany()
                  .HasForeignKey(e => e.User1Id)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.User2)
                  .WithMany()
                  .HasForeignKey(e => e.User2Id)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            // Prevent duplicate matches
            entity.HasIndex(e => new { e.User1Id, e.User2Id })
                  .IsUnique();
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
        
        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("messages", "public");
            
            entity.HasOne(m => m.Conversation)
                .WithMany()
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(m => m.ConversationId);
            entity.HasIndex(m => m.SentAt);
        });
        
        base.OnModelCreating(modelBuilder);
    }
}