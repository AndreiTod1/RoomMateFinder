using System.Security.Claims;
using MediatR;

namespace RoomMate_Finder.Features.RoomListings.CreateListing;

public static class CreateListingEndpoint
{
    public static IEndpointRouteBuilder MapCreateListingEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/room-listings", async (CreateListingRequest cmd, ClaimsPrincipal user, IMediator mediator) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var ownerId))
                {
                    return Results.Unauthorized();
                }

                cmd.OwnerId = ownerId;

                try
                {
                    var response = await mediator.Send(cmd);
                    return Results.Ok(response);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            })
            .RequireAuthorization()
            .WithTags("RoomListings")
            .WithName("CreateListing")
            .WithSummary("Create a new room listing for the current user")
            .Produces<CreateListingResponse>(200)
            .ProducesProblem(400)
            .ProducesProblem(401);

        return app;
    }
}

