using System.Security.Claims;
using MediatR;

namespace RoomMate_Finder.Features.Conversations.GetConversations;

public static class GetConversationsEndpoint
{
    public static IEndpointRouteBuilder MapGetConversationsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/conversations", async (
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

                var request = new GetConversationsRequest();
                var response = await mediator.Send(request);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "An error occurred while retrieving conversations"
                );
            }
        })
        .RequireAuthorization()
        .WithTags("Conversations")
        .WithName("GetConversations")
        .WithSummary("Get all conversations for the authenticated user")
        .Produces<GetConversationsResponse>()
        .ProducesProblem(401)
        .ProducesProblem(500);
        
        return app;
    }
}
