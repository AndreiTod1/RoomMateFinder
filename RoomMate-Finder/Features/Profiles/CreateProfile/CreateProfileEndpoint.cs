using MediatR;

namespace RoomMate_Finder.Features.Profiles;

public static class CreateProfileEndpoint 
{
    public static IEndpointRouteBuilder MapCreateProfileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/profiles", async (CreateProfileRequest cmd, IMediator mediator) =>
            {
                var id = await mediator.Send(cmd);
                return Results.Ok(new { ID = id });
            })
            .WithName("CreateProfile")
            .WithSummary("Creates a new user profile")
            .Produces(200)
            .ProducesProblem(400);
        return app;
    }
}