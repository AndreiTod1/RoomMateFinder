using RoomMate_Finder.Features.Admins.GetAdmins;
using RoomMate_Finder.Features.Admins.GetAllUsers;
using RoomMate_Finder.Features.Admins.DeleteProfile;
using RoomMate_Finder.Features.Admins.UpdateUserRole;

namespace RoomMate_Finder.Features.Admins;

public static class AdminsEndpoints
{
    public static IEndpointRouteBuilder MapAdminsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGetAdminsEndpoint();
        app.MapGetAllUsersEndpoint();
        app.MapDeleteProfileEndpoint();
        app.MapUpdateUserRoleEndpoint();

        return app;
    }
}
