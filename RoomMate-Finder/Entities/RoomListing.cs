using RoomMate_Finder.Entities;

namespace RoomMate_Finder.Entities;

public class RoomListing
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public Profile Owner { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime AvailableFrom { get; set; }
    public string Amenities { get; set; } = string.Empty; // comma-separated list
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
