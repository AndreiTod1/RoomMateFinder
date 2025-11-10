using RoomMate_Finder.Features.Profiles.Login;
using RoomMate_Finder.Features.Profiles.GetProfiles;
using RoomMate_Finder.Features.Profiles.GetProfileById;
using RoomMate_Finder.Features.Profiles.UpdateProfile;

namespace RoomMate_Finder.Features.Profiles;

public static class ProfilesEndpoints
{
    public static IEndpointRouteBuilder MapProfilesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateProfileEndpoint();
        app.MapLoginEndpoint();
        app.MapGetProfilesEndpoint();
        app.MapGetProfileByIdEndpoint();
        app.MapUpdateProfileEndpoint();
        
        return app;
    }
}