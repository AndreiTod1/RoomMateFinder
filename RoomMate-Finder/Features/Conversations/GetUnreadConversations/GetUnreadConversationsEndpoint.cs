using System.Security.Claims;
using MediatR;

namespace RoomMate_Finder.Features.Conversations.GetUnreadConversations;

public static class GetUnreadConversationsEndpoint
{
    public static IEndpointRouteBuilder MapGetUnreadConversationsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/conversations/unread", async (
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

                var request = new GetUnreadConversationsRequest();
                var response = await mediator.Send(request);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "An error occurred while retrieving unread conversations"
                );
            }
        })
        .RequireAuthorization()
        .WithTags("Conversations")
        .WithName("GetUnreadConversations")
        .WithSummary("Get all conversations with unread messages")
        .Produces<GetUnreadConversationsResponse>()
        .ProducesProblem(401)
        .ProducesProblem(500);
        
        return app;
    }
}

