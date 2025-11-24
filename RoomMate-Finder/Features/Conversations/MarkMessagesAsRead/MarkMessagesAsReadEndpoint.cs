using System.Security.Claims;
using MediatR;

namespace RoomMate_Finder.Features.Conversations.MarkMessagesAsRead;

public static class MarkMessagesAsReadEndpoint
{
    public static IEndpointRouteBuilder MapMarkMessagesAsReadEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/conversations/{conversationId:guid}/messages/mark-read", async (
            Guid conversationId,
            IMediator mediator,
            ClaimsPrincipal user,
            HttpContext httpContext) =>
        {
            try
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var authenticatedUserId))
                {
                    return Results.Unauthorized();
                }

                httpContext.Items["CurrentUserId"] = authenticatedUserId;

                var request = new MarkMessagesAsReadRequest(conversationId);
                var response = await mediator.Send(request);
                return Results.Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "An error occurred while marking messages as read"
                );
            }
        })
        .RequireAuthorization()
        .WithTags("Conversations")
        .WithName("MarkMessagesAsRead")
        .WithSummary("Mark all unread messages in a conversation as read")
        .Produces<MarkMessagesAsReadResponse>()
        .ProducesProblem(401)
        .ProducesProblem(403)
        .ProducesProblem(404)
        .ProducesProblem(500);
        
        return app;
    }
}
