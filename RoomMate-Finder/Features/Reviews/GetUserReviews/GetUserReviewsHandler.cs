using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Reviews.GetUserReviews;

public class GetUserReviewsHandler : IRequestHandler<GetUserReviewsRequest, GetUserReviewsResponse>
{
    private readonly AppDbContext _dbContext;

    public GetUserReviewsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetUserReviewsResponse> Handle(GetUserReviewsRequest request, CancellationToken cancellationToken)
    {
        var userExists = await _dbContext.Profiles.AnyAsync(p => p.Id == request.ReviewedUserId, cancellationToken);
        if (!userExists)
        {
            throw new KeyNotFoundException("User not found");
        }

        var reviews = await _dbContext.Reviews
            .Where(r => r.ReviewedUserId == request.ReviewedUserId)
            .Include(r => r.Reviewer)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        var dto = new GetUserReviewsResponse
        {
            Reviews = reviews.Select(r => new GetUserReviewsResponse.ReviewDto
            {
                Id = r.Id,
                ReviewerId = r.ReviewerId,
                ReviewerFullName = r.Reviewer.FullName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList()
        };

        return dto;
    }
}

