using MediatR;

namespace RoomMate_Finder.Features.Matching.CalculateCompatibility;

public static class CalculateCompatibilityEndpoint
{
    public static IEndpointRouteBuilder MapCalculateCompatibilityEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/matching/compatibility/{userId1:guid}/{userId2:guid}", 
            async (Guid userId1, Guid userId2, IMediator mediator) =>
            {
                try
                {
                    var request = new CalculateCompatibilityRequest(userId1, userId2);
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
            .WithName("CalculateCompatibility")
            .WithSummary("Calculate compatibility score between two users")
            .Produces<CalculateCompatibilityResponse>(200)
            .ProducesProblem(404)
            .ProducesProblem(400);

        return app;
    }
}
