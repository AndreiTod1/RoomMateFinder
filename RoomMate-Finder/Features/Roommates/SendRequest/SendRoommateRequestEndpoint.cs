using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace RoomMate_Finder.Features.Roommates.SendRequest;

public static class SendRoommateRequestEndpoint
{
    public static void MapSendRoommateRequestEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/roommates/requests", async ([FromBody] SendRoommateRequestDto dto, ISender sender) =>
        {
            try
            {
                var result = await sender.Send(new SendRoommateRequestRequest(dto.TargetUserId, dto.Message));
                return Results.Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .RequireAuthorization()
        .WithTags("Roommates");
    }
}

public record SendRoommateRequestDto(Guid TargetUserId, string? Message);

