using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using MediatR;
using RoomMate_Finder.Features.Reviews.GetReviewStats;

namespace RoomMate_Finder.Features.Reviews.GetReviewStats;

public static class GetReviewStatsEndpoint
{
    public static void MapGetReviewStatsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/profiles/{id:guid}/reviews/stats", async (Guid id, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(new GetReviewStatsRequest(id));
                return Results.Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithTags("Reviews")
        .WithName("GetReviewStats")
        .Produces<GetReviewStatsResponse>(200)
        .ProducesProblem(404);
    }
}

