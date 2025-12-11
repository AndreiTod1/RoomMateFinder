namespace RoomMate_Finder.Features.RoomListings.UpdateListing;

public class UpdateListingResponse
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string City { get; set; }
    public required string Area { get; set; }
    public decimal Price { get; set; }
    public DateTime AvailableFrom { get; set; }
    public List<string> Amenities { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}
