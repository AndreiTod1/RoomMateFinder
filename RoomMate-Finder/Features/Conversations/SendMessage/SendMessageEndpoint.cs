using System.Security.Claims;
using MediatR;

namespace RoomMate_Finder.Features.Conversations.SendMessage;

public static class SendMessageEndpoint
{
    public static IEndpointRouteBuilder MapSendMessageEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/conversations/{conversationId:guid}/messages", async (
            Guid conversationId,
            SendMessageRequestBody requestBody,
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

                if (string.IsNullOrWhiteSpace(requestBody.Content))
                {
                    return Results.BadRequest(new { message = "Message content cannot be empty" });
                }

                if (requestBody.Content.Length > 1000)
                {
                    return Results.BadRequest(new { message = "Message content cannot exceed 1000 characters" });
                }

                var request = new SendMessageRequest(conversationId, requestBody.Content);
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
                    title: "An error occurred while sending the message"
                );
            }
        })
        .RequireAuthorization()
        .WithTags("Conversations")
        .WithName("SendMessage")
        .WithSummary("Send a message in a conversation")
        .Produces<SendMessageResponse>()
        .ProducesProblem(400)
        .ProducesProblem(401)
        .ProducesProblem(403)
        .ProducesProblem(404)
        .ProducesProblem(500);
        
        return app;
    }
}

public record SendMessageRequestBody(string Content);
