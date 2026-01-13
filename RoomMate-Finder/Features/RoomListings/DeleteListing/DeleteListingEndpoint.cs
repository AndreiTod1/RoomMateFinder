using System.Security.Claims;
using MediatR;
using RoomMate_Finder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace RoomMate_Finder.Features.RoomListings.DeleteListing;

public static class DeleteListingEndpoint
{
    public static IEndpointRouteBuilder MapDeleteListingEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/room-listings/{id:guid}", async (Guid id, ClaimsPrincipal user, IMediator mediator) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var command = new DeleteListingCommand(id, userId);
                var result = await mediator.Send(command);

                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                        return Results.NotFound(new { message = result.Message });
                    if (result.Message.Contains("authorized"))
                        return Results.Forbid();
                    
                    return Results.BadRequest(new { message = result.Message });
                }

                return Results.Ok(new { message = result.Message });
            })
            .RequireAuthorization()
            .WithTags("RoomListings")
            .WithName("DeleteListing")
            .WithSummary("Delete a listing (owner or admin only)")
            .Produces(200)
            .ProducesProblem(401)
            .ProducesProblem(403)
            .ProducesProblem(404);

        return app;
    }
}
