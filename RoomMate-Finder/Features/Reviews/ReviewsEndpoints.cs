using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using RoomMate_Finder.Features.Reviews.CreateReview;
using RoomMate_Finder.Features.Reviews.GetReviewStats;
using RoomMate_Finder.Features.Reviews.GetUserReviews;

namespace RoomMate_Finder.Features.Reviews;

public static class ReviewsEndpoints
{
    public static void MapReviewsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateReviewEndpoint();
        app.MapGetUserReviewsEndpoint();
        app.MapGetReviewStatsEndpoint();
    }
}

