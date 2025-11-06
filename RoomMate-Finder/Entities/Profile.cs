namespace RoomMate_Finder.Entities;

public class Profile
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string University { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Lifestyle { get; set; } = string.Empty;
    public string Interests { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}