using RoomMate_Finder.Features.RoomListings.CreateListing;
using RoomMate_Finder.Features.RoomListings.GetListingById;
using RoomMate_Finder.Features.RoomListings.SearchListings;
using RoomMate_Finder.Features.RoomListings.UpdateListing;

namespace RoomMate_Finder.Features.RoomListings;

public static class RoomListingsEndpoints
{
    public static IEndpointRouteBuilder MapRoomListingsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateListingEndpoint();
        app.MapSearchListingsEndpoint();
        app.MapGetListingByIdEndpoint();
        app.MapUpdateListingEndpoint();

        return app;
    }
}
