using MediatR;

namespace RoomMate_Finder.Features.Roommates.GetMyRequests;

public static class GetMyRequestsEndpoint
{
    public static void MapGetMyRequestsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("api/roommates/my-requests", async (ISender sender) =>
        {
            try
            {
                var result = await sender.Send(new GetMyRequestsRequest());
                return Results.Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        })
        .RequireAuthorization()
        .WithTags("Roommates");
    }
}

