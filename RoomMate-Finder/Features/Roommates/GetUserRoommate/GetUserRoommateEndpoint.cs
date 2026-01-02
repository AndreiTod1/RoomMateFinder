using MediatR;

namespace RoomMate_Finder.Features.Roommates.GetUserRoommate;

public static class GetUserRoommateEndpoint
{
    public static void MapGetUserRoommateEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("api/roommates/user/{userId:guid}", async (Guid userId, ISender sender) =>
        {
            var result = await sender.Send(new GetUserRoommateRequest(userId));
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithTags("Roommates");
    }
}

