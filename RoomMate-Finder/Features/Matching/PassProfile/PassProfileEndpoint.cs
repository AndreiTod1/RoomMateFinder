using MediatR;

namespace RoomMate_Finder.Features.Matching.PassProfile;

public static class PassProfileEndpoint
{
    public static IEndpointRouteBuilder MapPassProfileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/matching/pass", 
            async (PassProfileRequest request, IMediator mediator) =>
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
            .WithName("PassProfile")
            .WithSummary("Pass (skip) another user's profile")
            .WithDescription("User indicates no interest in another user.")
            .Produces<PassProfileResponse>(200)
            .ProducesProblem(400);

        return app;
    }
}
