using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace RoomMate_Finder.Features.Admins.UpdateUserRole;

public static class UpdateUserRoleEndpoint
{
    public static void MapUpdateUserRoleEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("api/admins/users/{id}/role", async (Guid id, [FromBody] string role, ISender sender) =>
        {
            await sender.Send(new UpdateUserRoleRequest(id, role));
            return Results.NoContent();
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithTags("Admins");
    }
}
