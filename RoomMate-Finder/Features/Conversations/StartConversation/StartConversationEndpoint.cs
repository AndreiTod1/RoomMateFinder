using System.Security.Claims;
using FluentValidation;
using MediatR;

namespace RoomMate_Finder.Features.Conversations.StartConversation;

public static class StartConversationEndpoint
{
    public static IEndpointRouteBuilder MapStartConversationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/conversations", async (
            StartConversationRequest request,
            IMediator mediator,
            ClaimsPrincipal user,
            IValidator<StartConversationRequest> validator,
            HttpContext httpContext) =>
        {
            try
            {
                // Get the authenticated user's ID from JWT token
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var authenticatedUserId))
                {
                    return Results.Unauthorized();
                }

                // Store CurrentUserId in HttpContext.Items to pass to handler
                httpContext.Items["CurrentUserId"] = authenticatedUserId;

                // Validate the request
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                    return Results.BadRequest(new { message = errors });
                }

                var response = await mediator.Send(request);
                return Results.Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .RequireAuthorization()
        .WithTags("Conversations")
        .WithName("StartConversation")
        .WithSummary("Start a new conversation with another user")
        .Produces<StartConversationResponse>(200)
        .ProducesProblem(400)
        .ProducesProblem(401)
        .ProducesProblem(404);
        
        return app;
    }
}
