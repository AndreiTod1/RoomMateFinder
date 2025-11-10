using MediatR;

namespace RoomMate_Finder.Features.Profiles.GetProfiles;

public static class GetProfilesEndpoint
{
    public static IEndpointRouteBuilder MapGetProfilesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/profiles", async (IMediator mediator) =>
            {
                var request = new GetProfilesRequest();
                var response = await mediator.Send(request);
                return Results.Ok(response);
            })
            .WithTags("Profiles")
            .WithName("GetProfiles")
            .WithSummary("Gets all user profiles")
            .Produces<List<GetProfilesResponse>>(200);
            
        return app;
    }
}