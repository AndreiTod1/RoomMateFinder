using MediatR;

namespace RoomMate_Finder.Features.Roommates.DeleteRelationship;

public static class DeleteRelationshipEndpoint
{
    public static void MapDeleteRelationshipEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("api/roommates/relationships/{relationshipId}", async (Guid relationshipId, ISender sender) =>
        {
            try
            {
                var result = await sender.Send(new DeleteRelationshipRequest(relationshipId));
                return Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithTags("Roommates");
    }
}

