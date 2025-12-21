using MediatR;

namespace RoomMate_Finder.Features.Roommates.ApproveRequest;

public static class ApproveRequestEndpoint
{
    public static void MapApproveRequestEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/roommates/requests/{requestId}/approve", async (Guid requestId, ISender sender) =>
        {
            try
            {
                var result = await sender.Send(new ApproveRequestRequest(requestId));
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

