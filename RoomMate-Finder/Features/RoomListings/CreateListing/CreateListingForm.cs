namespace RoomMate_Finder.Features.RoomListings.CreateListing;

public class CreateListingForm
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime AvailableFrom { get; set; }
    public string Amenities { get; set; } = string.Empty; // comma-separated
    public List<IFormFile>? Images { get; set; }
}
