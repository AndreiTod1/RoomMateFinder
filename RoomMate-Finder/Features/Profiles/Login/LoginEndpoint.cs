using MediatR;

namespace RoomMate_Finder.Features.Profiles.Login;

public static class LoginEndpoint
{
    public static IEndpointRouteBuilder MapLoginEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/profiles/login", async (LoginRequest request, IMediator mediator) =>
            {
                try
                {
                    var response = await mediator.Send(request);
                    return Results.Ok(response);
                }
                catch (UnauthorizedAccessException)
                {
                    return Results.Unauthorized();
                }
            })
            .WithTags("Authentication")
            .WithName("LoginProfile")
            .WithSummary("Authenticates a user")
            .Produces<LoginResponse>(200)
            .ProducesProblem(401);
            
        return app;
    }
}
