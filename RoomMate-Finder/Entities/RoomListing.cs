using System.ComponentModel.DataAnnotations;

namespace RoomMate_Finder.Entities;

public class RoomListing
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public Profile Owner { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string City { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Area { get; set; } = string.Empty;
    
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }
    
    public DateTime AvailableFrom { get; set; }
    
    [MaxLength(500)] // Limitez pentru că e stocat ca string comma-separated
    public string Amenities { get; set; } = string.Empty; // comma-separated list
    
    [MaxLength(2000)] // Max 8 images * ~200 chars per path
    public string ImagePaths { get; set; } = string.Empty; // comma-separated list of image paths
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Keep for backward compatibility, will be removed in future migration
    public ICollection<RoomListingImage> Images { get; set; } = new List<RoomListingImage>();
}
