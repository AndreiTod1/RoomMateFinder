using RoomMate_Finder.Features.Roommates.SendRequest;
using RoomMate_Finder.Features.Roommates.GetPendingRequests;
using RoomMate_Finder.Features.Roommates.ApproveRequest;
using RoomMate_Finder.Features.Roommates.RejectRequest;
using RoomMate_Finder.Features.Roommates.GetRelationships;
using RoomMate_Finder.Features.Roommates.DeleteRelationship;
using RoomMate_Finder.Features.Roommates.GetMyRequests;

namespace RoomMate_Finder.Features.Roommates;

public static class RoommatesEndpoints
{
    public static IEndpointRouteBuilder MapRoommatesEndpoints(this IEndpointRouteBuilder app)
    {
        // User endpoints
        app.MapSendRoommateRequestEndpoint();
        app.MapGetMyRequestsEndpoint();
        
        // Admin endpoints
        app.MapGetPendingRequestsEndpoint();
        app.MapApproveRequestEndpoint();
        app.MapRejectRequestEndpoint();
        app.MapGetRelationshipsEndpoint();
        app.MapDeleteRelationshipEndpoint();

        return app;
    }
}

