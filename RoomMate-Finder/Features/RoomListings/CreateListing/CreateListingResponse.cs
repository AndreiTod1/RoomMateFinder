using RoomMate_Finder.Entities;

namespace RoomMate_Finder.Features.RoomListings.CreateListing;

public class CreateListingResponse
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime AvailableFrom { get; set; }
    public List<string> Amenities { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public List<string> ImagePaths { get; set; } = new();
    public ListingApprovalStatus ApprovalStatus { get; set; }
}

