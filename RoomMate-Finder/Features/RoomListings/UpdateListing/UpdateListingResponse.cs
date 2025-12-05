namespace RoomMate_Finder.Features.RoomListings.UpdateListing;

public class UpdateListingResponse
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string City { get; set; }
    public string Area { get; set; }
    public decimal Price { get; set; }
    public DateTime AvailableFrom { get; set; }
    public List<string> Amenities { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}
