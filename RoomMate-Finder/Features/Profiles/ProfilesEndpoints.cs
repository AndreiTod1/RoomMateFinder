using RoomMate_Finder.Features.Profiles.Login;

namespace RoomMate_Finder.Features.Profiles;

public static class ProfilesEndpoints
{
    public static IEndpointRouteBuilder MapProfilesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateProfileEndpoint();
        app.MapLoginEndpoint();
        return app;
    }
}