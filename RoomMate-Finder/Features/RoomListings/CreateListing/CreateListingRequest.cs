using MediatR;

namespace RoomMate_Finder.Features.RoomListings.CreateListing;

public class CreateListingRequest : IRequest<CreateListingResponse>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime AvailableFrom { get; set; }
    public List<string> Amenities { get; set; } = new();
    public Guid OwnerId { get; set; }
}

