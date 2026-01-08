using System.Security.Claims;
using MediatR;

namespace RoomMate_Finder.Features.RoomListings.ApproveRejectListing;

public static class ApproveRejectListingEndpoints
{
    public static IEndpointRouteBuilder MapApproveRejectListingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/listings/{id:guid}/approve", async (
            Guid id,
            ClaimsPrincipal user,
            IMediator mediator) =>
        {
            var adminIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
            {
                return Results.Unauthorized();
            }

            var result = await mediator.Send(new ApproveListingCommand(id, adminId));
            
            if (!result.Success)
            {
                return Results.BadRequest(new { message = result.Message });
            }
            
            return Results.Ok(result);
        })
        .WithName("ApproveListing")
        .WithTags("Room Listings")
        .RequireAuthorization(policy => policy.RequireRole("Admin"));

        app.MapPost("/api/listings/{id:guid}/reject", async (
            Guid id,
            RejectListingRequest request,
            ClaimsPrincipal user,
            IMediator mediator) =>
        {
            var adminIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
            {
                return Results.Unauthorized();
            }

            var result = await mediator.Send(new RejectListingCommand(id, adminId, request.Reason));
            
            if (!result.Success)
            {
                return Results.BadRequest(new { message = result.Message });
            }
            
            return Results.Ok(result);
        })
        .WithName("RejectListing")
        .WithTags("Room Listings")
        .RequireAuthorization(policy => policy.RequireRole("Admin"));

        return app;
    }
}

public record RejectListingRequest(string Reason);

