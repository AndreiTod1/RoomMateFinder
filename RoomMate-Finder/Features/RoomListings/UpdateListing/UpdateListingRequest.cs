using MediatR;

namespace RoomMate_Finder.Features.RoomListings.UpdateListing;

public class UpdateListingRequest : IRequest<UpdateListingResponse?>
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
    public bool IsActive { get; set; } = true;
}

