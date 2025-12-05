namespace RoomMate_Finder.Features.RoomListings.SearchListings;

public class RoomListingSummaryDto
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerFullName { get; set; }
    public string Title { get; set; }
    public string City { get; set; }
    public string Area { get; set; }
    public decimal Price { get; set; }
    public DateTime AvailableFrom { get; set; }
    public List<string> Amenities { get; set; }
}

