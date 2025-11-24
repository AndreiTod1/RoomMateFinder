namespace RoomMate_Finder.Entities;

public class Conversation
{
    public Guid Id { get; set; }
    public Guid User1Id { get; set; }
    public Guid User2Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Profile User1 { get; set; } = null!;
    public Profile User2 { get; set; } = null!;
}

