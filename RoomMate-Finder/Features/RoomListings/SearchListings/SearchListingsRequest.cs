using MediatR;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.RoomListings.SearchListings;

namespace RoomMate_Finder.Features.RoomListings.SearchListings;

public class SearchListingsRequest : IRequest<SearchListingsResponse>
{
    public string? City { get; set; }
    public string? Area { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<string>? Amenities { get; set; }
    public DateTime? AvailableFrom { get; set; }
    public Guid? OwnerId { get; set; }
    public bool IncludeInactive { get; set; } = false;
    public ListingApprovalStatus? ApprovalStatus { get; set; }
    public bool IncludePending { get; set; } = false;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

