using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using MediatR;
using RoomMate_Finder.Features.Reviews.GetUserReviews;

namespace RoomMate_Finder.Features.Reviews.GetUserReviews;

public static class GetUserReviewsEndpoint
{
    public static void MapGetUserReviewsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/profiles/{id:guid}/reviews", async (Guid id, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetUserReviewsRequest(id));
                return Results.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithTags("Reviews")
        .WithName("GetUserReviews")
        .Produces<GetUserReviewsResponse>(200)
        .ProducesProblem(404);
    }
}

