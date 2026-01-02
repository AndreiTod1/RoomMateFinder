using MediatR;

namespace RoomMate_Finder.Features.Roommates.GetRelationships;

public static class GetRelationshipsEndpoint
{
    public static void MapGetRelationshipsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("api/roommates/relationships", async (ISender sender) =>
        {
            var result = await sender.Send(new GetRelationshipsRequest());
            return Results.Ok(result);
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithTags("Roommates");
    }
}

