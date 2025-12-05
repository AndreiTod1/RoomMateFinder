using MediatR;

namespace RoomMate_Finder.Features.RoomListings.GetListingById;

public static class GetListingByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetListingByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/room-listings/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var response = await mediator.Send(new GetListingByIdRequest(id));
                if (response == null)
                {
                    return Results.NotFound(new { message = "Listing not found" });
                }

                return Results.Ok(response);
            })
            .WithTags("RoomListings")
            .WithName("GetListingById")
            .WithSummary("Get full room listing details by id")
            .Produces<GetListingByIdResponse>()
            .ProducesProblem(404);

        return app;
    }
}

