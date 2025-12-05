using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Reviews.GetReviewStats;

public class GetReviewStatsHandler : IRequestHandler<GetReviewStatsRequest, GetReviewStatsResponse>
{
    private readonly AppDbContext _dbContext;

    public GetReviewStatsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetReviewStatsResponse> Handle(GetReviewStatsRequest request, CancellationToken cancellationToken)
    {
        var userExists = await _dbContext.Profiles.AnyAsync(p => p.Id == request.ReviewedUserId, cancellationToken);
        if (!userExists)
        {
            throw new KeyNotFoundException("User not found");
        }

        var reviewsQuery = _dbContext.Reviews
            .Where(r => r.ReviewedUserId == request.ReviewedUserId);

        var total = await reviewsQuery.CountAsync(cancellationToken);

        var avg = 0.0;
        if (total > 0)
        {
            avg = await reviewsQuery.AverageAsync(r => (double)r.Rating, cancellationToken);
        }

        var distribution = await reviewsQuery
            .GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var distDict = distribution.ToDictionary(x => x.Rating, x => x.Count);

        // Ensure keys 1..5 exist
        for (int i = 1; i <= 5; i++)
        {
            if (!distDict.ContainsKey(i)) distDict[i] = 0;
        }

        return new GetReviewStatsResponse
        {
            ReviewedUserId = request.ReviewedUserId,
            AverageRating = Math.Round(avg, 2),
            TotalReviews = total,
            RatingDistribution = distDict
        };
    }
}

