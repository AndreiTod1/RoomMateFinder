using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace RoomMate_Finder.Features.Admins.GetAdmins;

public static class GetAdminsEndpoint
{
    public static void MapGetAdminsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("api/admins", async (ISender sender) =>
        {
            var response = await sender.Send(new GetAdminsRequest());
            return Results.Ok(response);
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithTags("Admins");
    }
}
