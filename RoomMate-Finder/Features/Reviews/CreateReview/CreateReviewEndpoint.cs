using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;
using System.Security.Claims;
using RoomMate_Finder.Features.Reviews.CreateReview;
using RoomMate_Finder.Validators;

namespace RoomMate_Finder.Features.Reviews.CreateReview;

public static class CreateReviewEndpoint
{
    public static void MapCreateReviewEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/profiles/{reviewedUserId:guid}/reviews", async (Guid reviewedUserId, CreateReviewRequestBody body, IHttpContextAccessor httpContextAccessor, IMediator mediator) =>
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity?.IsAuthenticated == true)
            {
                return Results.Unauthorized();
            }

            var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
            if (!Guid.TryParse(userIdStr, out var reviewerId))
            {
                return Results.Unauthorized();
            }

            var request = new CreateReviewRequest
            {
                ReviewerId = reviewerId,
                ReviewedUserId = reviewedUserId,
                Rating = body.Rating,
                Comment = body.Comment ?? string.Empty
            };

            try
            {
                var result = await mediator.Send(request);
                return Results.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }

        })
        .RequireAuthorization()
        .WithTags("Reviews")
        .WithName("CreateReview")
        .Produces<CreateReviewResponse>(200)
        .ProducesProblem(400)
        .ProducesProblem(401)
        .ProducesProblem(404);
    }

    public record CreateReviewRequestBody(int Rating, string? Comment);
}

