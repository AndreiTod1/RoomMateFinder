namespace RoomMate_Finder.Entities;

public class Message
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    
    // Navigation properties
    public Conversation Conversation { get; set; } = null!;
    public Profile Sender { get; set; } = null!;
}
