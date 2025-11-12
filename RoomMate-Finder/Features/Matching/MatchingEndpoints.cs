using RoomMate_Finder.Features.Matching.CalculateCompatibility;
using RoomMate_Finder.Features.Matching.GetMatches;
using RoomMate_Finder.Features.Matching.GetUserMatches;
using RoomMate_Finder.Features.Matching.LikeProfile;
using RoomMate_Finder.Features.Matching.PassProfile;

namespace RoomMate_Finder.Features.Matching;

public static class MatchingEndpoints
{
    public static IEndpointRouteBuilder MapMatchingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCalculateCompatibilityEndpoint();
        app.MapGetMatchesEndpoint();
        app.MapGetUserMatchesEndpoint();
        app.MapLikeProfileEndpoint();
        app.MapPassProfileEndpoint();
        
        return app;
    }
}
