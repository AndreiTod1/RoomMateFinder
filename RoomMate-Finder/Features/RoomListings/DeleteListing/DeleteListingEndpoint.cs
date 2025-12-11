using System.Security.Claims;
using MediatR;
using RoomMate_Finder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace RoomMate_Finder.Features.RoomListings.DeleteListing;

public static class DeleteListingEndpoint
{
    public static IEndpointRouteBuilder MapDeleteListingEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/room-listings/{id:guid}", async (Guid id, ClaimsPrincipal user, AppDbContext db) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var listing = await db.RoomListings.FirstOrDefaultAsync(l => l.Id == id);
                if (listing == null)
                {
                    return Results.NotFound(new { message = "Listing not found" });
                }

                // Check if user is owner OR admin
                var userProfile = await db.Profiles.FirstOrDefaultAsync(p => p.Id == userId);
                var isAdmin = userProfile?.Role == "Admin";
                
                if (listing.OwnerId != userId && !isAdmin)
                {
                    return Results.Forbid();
                }

                db.RoomListings.Remove(listing);
                await db.SaveChangesAsync();

                return Results.Ok(new { message = "Listing deleted successfully" });
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
