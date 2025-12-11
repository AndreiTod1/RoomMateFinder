using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace RoomMate_Finder.Features.Admins.GetAllUsers;

public static class GetAllUsersEndpoint
{
    public static void MapGetAllUsersEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("api/admins/users", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? search,
            ISender sender) =>
        {
            var response = await sender.Send(new GetAllUsersRequest(
                page > 0 ? page : 1,
                pageSize > 0 ? pageSize : 12,
                search));
            return Results.Ok(response);
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithTags("Admins");
    }
}
