using MediatR;

namespace RoomMate_Finder.Features.Matching.GetMatches;

public static class GetMatchesEndpoint
{
    public static IEndpointRouteBuilder MapGetMatchesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/matching/matches/{userId:guid}", 
            async (Guid userId, IMediator mediator) =>
            {
                try
                {
                    var request = new GetMatchesRequest(userId);
                    var response = await mediator.Send(request);
                    return Results.Ok(response);
                }
                catch (ArgumentException ex)
                {
                    return Results.NotFound(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            })
            .WithTags("Matching")
            .WithName("GetMatches")
            .WithSummary("Get compatibility matches for a user")
            .WithDescription("Returns a list of potential roommate matches sorted by compatibility score (highest first)")
            .Produces<List<GetMatchesResponse>>(200)
            .ProducesProblem(404)
            .ProducesProblem(400);

        return app;
    }
}
