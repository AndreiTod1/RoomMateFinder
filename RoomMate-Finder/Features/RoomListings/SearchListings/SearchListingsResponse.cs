namespace RoomMate_Finder.Features.RoomListings.SearchListings;

public class SearchListingsResponse
{
    public List<RoomListingSummaryDto> Listings { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}



