using MediatR;
using RoomMate_Finder.Features.Profiles.Login;

namespace RoomMate_Finder.Features.Profiles;

public static class CreateProfileEndpoint 
{
    public static IEndpointRouteBuilder MapCreateProfileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/profiles", async (CreateProfileRequest cmd, IMediator mediator) =>
            {
                try 
                {
                    var response = await mediator.Send(cmd);
                    return Results.Ok(response);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            })
            .WithTags("Authentication")
            .WithName("CreateProfile")
            .WithSummary("Creates a new user profile")
            .Produces<AuthResponse>(200)
            .ProducesProblem(400);
            
        return app;
    }
}