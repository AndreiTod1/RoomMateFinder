using MediatR;

namespace RoomMate_Finder.Features.Roommates.RejectRequest;

public static class RejectRequestEndpoint
{
    public static void MapRejectRequestEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/roommates/requests/{requestId}/reject", async (Guid requestId, ISender sender) =>
        {
            try
            {
                var result = await sender.Send(new RejectRequestRequest(requestId));
                return Results.Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithTags("Roommates");
    }
}

