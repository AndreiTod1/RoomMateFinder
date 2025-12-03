using MediatR;
using RoomMate_Finder.Infrastructure.Persistence;
using RoomMate_Finder.Entities;
using Microsoft.EntityFrameworkCore;

namespace RoomMate_Finder.Features.Reviews.CreateReview;

public class CreateReviewHandler : IRequestHandler<CreateReviewRequest, CreateReviewResponse>
{
    private readonly AppDbContext _dbContext;

    public CreateReviewHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateReviewResponse> Handle(CreateReviewRequest request, CancellationToken cancellationToken)
    {
        // Ensure reviewed user exists
        var reviewedUser = await _dbContext.Profiles
            .FirstOrDefaultAsync(p => p.Id == request.ReviewedUserId, cancellationToken);

        if (reviewedUser == null)
        {
            throw new KeyNotFoundException("Reviewed user not found");
        }

        if (request.ReviewerId == request.ReviewedUserId)
        {
            throw new InvalidOperationException("Cannot review yourself");
        }

        // Check for existing review
        var exists = await _dbContext.Reviews
            .AnyAsync(r => r.ReviewerId == request.ReviewerId && r.ReviewedUserId == request.ReviewedUserId, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Review already exists from this user");
        }

        var review = new Review
        {
            Id = Guid.NewGuid(),
            ReviewerId = request.ReviewerId,
            ReviewedUserId = request.ReviewedUserId,
            Rating = request.Rating,
            Comment = request.Comment ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Reviews.Add(review);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateReviewResponse
        {
            Id = review.Id,
            ReviewerId = review.ReviewerId,
            ReviewedUserId = review.ReviewedUserId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }
}

