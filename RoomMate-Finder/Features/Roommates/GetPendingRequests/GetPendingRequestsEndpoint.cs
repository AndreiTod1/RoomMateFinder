using MediatR;

namespace RoomMate_Finder.Features.Roommates.GetPendingRequests;

public static class GetPendingRequestsEndpoint
{
    public static void MapGetPendingRequestsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("api/roommates/requests/pending", async (ISender sender) =>
        {
            var result = await sender.Send(new GetPendingRequestsRequest());
            return Results.Ok(result);
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithTags("Roommates");
    }
}

