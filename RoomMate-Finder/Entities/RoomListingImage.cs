using System.ComponentModel.DataAnnotations;

namespace RoomMate_Finder.Entities;

public class RoomListingImage
{
    public Guid Id { get; set; }
    
    public Guid RoomListingId { get; set; }
    public RoomListing RoomListing { get; set; } = null!;
    
    [Required]
    [MaxLength(500)]
    public string ImagePath { get; set; } = string.Empty;
    
    public int DisplayOrder { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
