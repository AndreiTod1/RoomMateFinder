using Microsoft.AspNetCore.Mvc;
using MediatR;
using RoomMate_Finder.Features.RoomListings.SearchListings;

namespace RoomMate_Finder.Features.RoomListings.SearchListings;

public static class SearchListingsEndpoint
{
    public static IEndpointRouteBuilder MapSearchListingsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/room-listings/search", async ([FromBody] SearchListingsRequest request, IMediator mediator) =>
            {
                var response = await mediator.Send(request);
                return Results.Ok(response);
            })
            .WithTags("RoomListings")
            .WithName("SearchListings")
            .WithSummary("Search room listings with filters and pagination")
            .Produces<SearchListingsResponse>();

        return app;
    }
}
