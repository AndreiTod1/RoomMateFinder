using System.Security.Claims;
using MediatR;

namespace RoomMate_Finder.Features.RoomListings.UpdateListing;

public static class UpdateListingEndpoint
{
    public static IEndpointRouteBuilder MapUpdateListingEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/room-listings/{id:guid}", async (Guid id, UpdateListingRequest cmd, ClaimsPrincipal user, IMediator mediator) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var ownerId))
                {
                    return Results.Unauthorized();
                }

                cmd.Id = id;
                cmd.OwnerId = ownerId;

                var response = await mediator.Send(cmd);
                if (response == null)
                {
                    return Results.NotFound(new { message = "Listing not found" });
                }

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("RoomListings")
            .WithName("UpdateListing")
            .WithSummary("Update an existing listing owned by the current user")
            .Produces<UpdateListingResponse>()
            .ProducesProblem(401)
            .ProducesProblem(404);

        return app;
    }
}

