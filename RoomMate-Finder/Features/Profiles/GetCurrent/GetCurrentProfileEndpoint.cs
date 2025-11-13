using System.Security.Claims;
using MediatR;
using RoomMate_Finder.Features.Profiles.GetProfileById;

namespace RoomMate_Finder.Features.Profiles.GetCurrent;

public static class GetCurrentProfileEndpoint
{
    public static IEndpointRouteBuilder MapGetCurrentProfileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/profiles/me", async (ClaimsPrincipal user, IMediator mediator) =>
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var id))
                {
                    return Results.Unauthorized();
                }

                var req = new GetProfileByIdRequest(id);
                var response = await mediator.Send(req);
                if (response == null)
                {
                    return Results.NotFound(new { message = "Profile not found" });
                }

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("Profiles")
            .WithName("GetCurrentProfile")
            .WithSummary("Gets current authenticated user's profile")
            .Produces<GetProfileByIdResponse>()
            .ProducesProblem(401)
            .ProducesProblem(404);

        return app;
    }
}
