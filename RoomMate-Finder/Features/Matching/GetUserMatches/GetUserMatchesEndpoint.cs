using MediatR;

namespace RoomMate_Finder.Features.Matching.GetUserMatches;

public static class GetUserMatchesEndpoint
{
    public static IEndpointRouteBuilder MapGetUserMatchesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/matching/my-matches/{userId:guid}", 
            async (Guid userId, IMediator mediator) =>
            {
                try
                {
                    var request = new GetUserMatchesRequest(userId);
                    var response = await mediator.Send(request);
                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            })
            .WithTags("Matching")
            .WithName("GetUserMatches")
            .WithSummary("Get all matches for a user")
            .WithDescription("Returns a list of users that have mutual likes with the specified user")
            .Produces<List<GetUserMatchesResponse>>(200)
            .ProducesProblem(400);

        return app;
    }
}
