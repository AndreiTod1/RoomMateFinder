using MediatR;

namespace RoomMate_Finder.Features.Matching.LikeProfile;

public static class LikeProfileEndpoint
{
    public static IEndpointRouteBuilder MapLikeProfileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/matching/like", 
            async (LikeProfileRequest request, IMediator mediator) =>
            {
                try
                {
                    var response = await mediator.Send(request);
                    return response.Success ? Results.Ok(response) : Results.BadRequest(response);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            })
            .WithTags("Matching")
            .WithName("LikeProfile")
            .WithSummary("Like another user's profile")
            .WithDescription("User expresses interest in another user. If mutual, creates a match.")
            .Produces<LikeProfileResponse>(200)
            .ProducesProblem(400);

        return app;
    }
}
