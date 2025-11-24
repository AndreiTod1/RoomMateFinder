using System.Security.Claims;
using MediatR;

namespace RoomMate_Finder.Features.Conversations.GetMessages;

public static class GetMessagesEndpoint
{
    public static IEndpointRouteBuilder MapGetMessagesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/conversations/{conversationId:guid}/messages", async (
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

                var request = new GetMessagesRequest(conversationId);
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
                    title: "An error occurred while retrieving messages"
                );
            }
        })
        .RequireAuthorization()
        .WithTags("Conversations")
        .WithName("GetMessages")
        .WithSummary("Get all messages in a conversation")
        .Produces<GetMessagesResponse>()
        .ProducesProblem(401)
        .ProducesProblem(403)
        .ProducesProblem(404)
        .ProducesProblem(500);
        
        return app;
    }
}
