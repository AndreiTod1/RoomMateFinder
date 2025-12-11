using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace RoomMate_Finder.Features.Admins.DeleteProfile;

public static class DeleteProfileEndpoint
{
    public static void MapDeleteProfileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("api/admins/users/{id}", async (Guid id, ISender sender) =>
        {
            await sender.Send(new DeleteProfileRequest(id));
            return Results.NoContent();
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithTags("Admins");
    }
}
