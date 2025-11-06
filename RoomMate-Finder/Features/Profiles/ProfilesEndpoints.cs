namespace RoomMate_Finder.Features.Profiles;

public static class ProfilesEndpoints
{
    public static IEndpointRouteBuilder MapProfilesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateProfileEndpoint();
        // additional profile-related endpoints can be mapped here
        return app;
    }
}