using FluentAssertions;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Reviews.GetReviewStats;
using RoomMate_Finder.Infrastructure.Persistence;
using RoomMate_Finder.Test.Helpers;

namespace RoomMate_Finder.Test.Handlers.Reviews;

public class GetReviewStatsHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;

    public GetReviewStatsHandlerTests()
    {
        _dbContext = DbContextHelper.CreateInMemoryDbContext();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_UserWithReviews_ReturnsCorrectStats()
    {
        // Arrange
        var reviewedUserId = Guid.NewGuid();

        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = "reviewed@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Reviewed User",
            Bio = "Getting statistical analysis",
            Age = 25,
            Gender = "Female",
            University = "Stats University",
            Lifestyle = "Analytical",
            Interests = "Statistics, Data",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        await _dbContext.Profiles.AddAsync(reviewedUser);

        // Add reviewers and reviews with known distribution:
        // 2x rating 5, 1x rating 4, 2x rating 3, 1x rating 2, 1x rating 1
        // Average should be: (5*2 + 4*1 + 3*2 + 2*1 + 1*1) / 7 = (10+4+6+2+1)/7 = 23/7 ≈ 3.29
        var reviewData = new[]
        {
            new { Rating = 5, Comment = "Excellent" },
            new { Rating = 5, Comment = "Outstanding" },
            new { Rating = 4, Comment = "Good" },
            new { Rating = 3, Comment = "Average" },
            new { Rating = 3, Comment = "Okay" },
            new { Rating = 2, Comment = "Below average" },
            new { Rating = 1, Comment = "Poor" }
        };

        var profiles = new List<Profile>();
        var reviews = new List<Review>();

        for (int i = 0; i < reviewData.Length; i++)
        {
            var reviewerId = Guid.NewGuid();
            var data = reviewData[i];

            var reviewer = new Profile
            {
                Id = reviewerId,
                Email = $"reviewer{i + 1}@stats.com",
                PasswordHash = "hashedpassword",
                FullName = $"Reviewer {i + 1}",
                Bio = "Statistical reviewer",
                Age = 25 + i,
                Gender = i % 2 == 0 ? "Male" : "Female",
                University = "Stats University",
                Lifestyle = "Active",
                Interests = "Reviewing",
                Role = "User",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };

            var review = new Review
            {
                Id = Guid.NewGuid(),
                ReviewerId = reviewerId,
                ReviewedUserId = reviewedUserId,
                Rating = data.Rating,
                Comment = data.Comment,
                CreatedAt = DateTime.UtcNow.AddDays(-(i + 1))
            };

            profiles.Add(reviewer);
            reviews.Add(review);
        }

        await _dbContext.Profiles.AddRangeAsync(profiles);
        await _dbContext.Reviews.AddRangeAsync(reviews);
        await _dbContext.SaveChangesAsync();

        var request = new GetReviewStatsRequest(reviewedUserId);
        var handler = new GetReviewStatsHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ReviewedUserId.Should().Be(reviewedUserId);
        result.TotalReviews.Should().Be(7);
        result.AverageRating.Should().BeApproximately(3.29, 0.01); // (23/7)

        result.RatingDistribution.Should().HaveCount(5);
        result.RatingDistribution[1].Should().Be(1); // 1 review with rating 1
        result.RatingDistribution[2].Should().Be(1); // 1 review with rating 2
        result.RatingDistribution[3].Should().Be(2); // 2 reviews with rating 3
        result.RatingDistribution[4].Should().Be(1); // 1 review with rating 4
        result.RatingDistribution[5].Should().Be(2); // 2 reviews with rating 5
    }

    [Fact]
    public async Task Handle_UserWithoutReviews_ReturnsZeroStats()
    {
        // Arrange
        var userWithoutReviewsId = Guid.NewGuid();

        var userWithoutReviews = new Profile
        {
            Id = userWithoutReviewsId,
            Email = "noreviews@example.com",
            PasswordHash = "hashedpassword",
            FullName = "No Reviews User",
            Bio = "Hasn't been reviewed yet",
            Age = 22,
            Gender = "Male",
            University = "University",
            Lifestyle = "Quiet",
            Interests = "Being invisible",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        await _dbContext.Profiles.AddAsync(userWithoutReviews);
        await _dbContext.SaveChangesAsync();

        var request = new GetReviewStatsRequest(userWithoutReviewsId);
        var handler = new GetReviewStatsHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ReviewedUserId.Should().Be(userWithoutReviewsId);
        result.TotalReviews.Should().Be(0);
        result.AverageRating.Should().Be(0);
        result.RatingDistribution.Should().HaveCount(5);
        result.RatingDistribution[1].Should().Be(0);
        result.RatingDistribution[2].Should().Be(0);
        result.RatingDistribution[3].Should().Be(0);
        result.RatingDistribution[4].Should().Be(0);
        result.RatingDistribution[5].Should().Be(0);
    }

    [Fact]
    public async Task Handle_NonExistentUser_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();

        var request = new GetReviewStatsRequest(nonExistentUserId);
        var handler = new GetReviewStatsHandler(_dbContext);

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found");
    }

    [Fact]
    public async Task Handle_UserWithSingleReview_ReturnsCorrectStats()
    {
        // Arrange
        var reviewedUserId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();

        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = "singlereview@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Single Review User",
            Bio = "Only one review",
            Age = 24,
            Gender = "Female",
            University = "University",
            Lifestyle = "Simple",
            Interests = "Simplicity",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-40)
        };

        var reviewer = new Profile
        {
            Id = reviewerId,
            Email = "onlyreviewer@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Only Reviewer",
            Bio = "The only one who reviewed",
            Age = 26,
            Gender = "Male",
            University = "University",
            Lifestyle = "Active",
            Interests = "Being the only one",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-50)
        };

        var singleReview = new Review
        {
            Id = Guid.NewGuid(),
            ReviewerId = reviewerId,
            ReviewedUserId = reviewedUserId,
            Rating = 4,
            Comment = "The only review for this user",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        await _dbContext.Profiles.AddRangeAsync(reviewedUser, reviewer);
        await _dbContext.Reviews.AddAsync(singleReview);
        await _dbContext.SaveChangesAsync();

        var request = new GetReviewStatsRequest(reviewedUserId);
        var handler = new GetReviewStatsHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ReviewedUserId.Should().Be(reviewedUserId);
        result.TotalReviews.Should().Be(1);
        result.AverageRating.Should().Be(4.0); // Only one review with rating 4

        result.RatingDistribution.Should().HaveCount(5);
        result.RatingDistribution[4].Should().Be(1); // 1 review with rating 4
        result.RatingDistribution[1].Should().Be(0);
        result.RatingDistribution[2].Should().Be(0);
        result.RatingDistribution[3].Should().Be(0);
        result.RatingDistribution[5].Should().Be(0);
    }

    [Fact]
    public async Task Handle_UserWithPerfectRatings_ReturnsCorrectStats()
    {
        // Arrange
        var reviewedUserId = Guid.NewGuid();

        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = "perfect@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Perfect User",
            Bio = "Gets only perfect ratings",
            Age = 25,
            Gender = "Other",
            University = "Perfect University",
            Lifestyle = "Perfect",
            Interests = "Perfection",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        await _dbContext.Profiles.AddAsync(reviewedUser);

        var profiles = new List<Profile>();
        var reviews = new List<Review>();

        // Add 5 perfect reviews (all rating 5)
        for (int i = 0; i < 5; i++)
        {
            var reviewerId = Guid.NewGuid();

            var reviewer = new Profile
            {
                Id = reviewerId,
                Email = $"perfectreviewer{i + 1}@example.com",
                PasswordHash = "hashedpassword",
                FullName = $"Perfect Reviewer {i + 1}",
                Bio = "Gives perfect scores",
                Age = 25 + i,
                Gender = "Male",
                University = "Perfect University",
                Lifestyle = "Active",
                Interests = "Perfect reviews",
                Role = "User",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };

            var review = new Review
            {
                Id = Guid.NewGuid(),
                ReviewerId = reviewerId,
                ReviewedUserId = reviewedUserId,
                Rating = 5,
                Comment = $"Perfect roommate #{i + 1}",
                CreatedAt = DateTime.UtcNow.AddDays(-(i + 1))
            };

            profiles.Add(reviewer);
            reviews.Add(review);
        }

        await _dbContext.Profiles.AddRangeAsync(profiles);
        await _dbContext.Reviews.AddRangeAsync(reviews);
        await _dbContext.SaveChangesAsync();

        var request = new GetReviewStatsRequest(reviewedUserId);
        var handler = new GetReviewStatsHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ReviewedUserId.Should().Be(reviewedUserId);
        result.TotalReviews.Should().Be(5);
        result.AverageRating.Should().Be(5.0); // All perfect ratings

        result.RatingDistribution.Should().HaveCount(5);
        result.RatingDistribution[5].Should().Be(5); // All 5 reviews have rating 5
        result.RatingDistribution[1].Should().Be(0);
        result.RatingDistribution[2].Should().Be(0);
        result.RatingDistribution[3].Should().Be(0);
        result.RatingDistribution[4].Should().Be(0);
    }

    [Fact]
    public async Task Handle_UserWithWorstRatings_ReturnsCorrectStats()
    {
        // Arrange
        var reviewedUserId = Guid.NewGuid();

        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = "worst@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Worst User",
            Bio = "Gets terrible ratings",
            Age = 25,
            Gender = "Male",
            University = "Bad University",
            Lifestyle = "Problematic",
            Interests = "Being difficult",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        await _dbContext.Profiles.AddAsync(reviewedUser);

        var profiles = new List<Profile>();
        var reviews = new List<Review>();

        // Add 3 terrible reviews (all rating 1)
        for (int i = 0; i < 3; i++)
        {
            var reviewerId = Guid.NewGuid();

            var reviewer = new Profile
            {
                Id = reviewerId,
                Email = $"harshreviewer{i + 1}@example.com",
                PasswordHash = "hashedpassword",
                FullName = $"Harsh Reviewer {i + 1}",
                Bio = "Gives harsh but honest scores",
                Age = 25 + i,
                Gender = "Female",
                University = "Honest University",
                Lifestyle = "Critical",
                Interests = "Honest feedback",
                Role = "User",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };

            var review = new Review
            {
                Id = Guid.NewGuid(),
                ReviewerId = reviewerId,
                ReviewedUserId = reviewedUserId,
                Rating = 1,
                Comment = $"Terrible experience #{i + 1}",
                CreatedAt = DateTime.UtcNow.AddDays(-(i + 1))
            };

            profiles.Add(reviewer);
            reviews.Add(review);
        }

        await _dbContext.Profiles.AddRangeAsync(profiles);
        await _dbContext.Reviews.AddRangeAsync(reviews);
        await _dbContext.SaveChangesAsync();

        var request = new GetReviewStatsRequest(reviewedUserId);
        var handler = new GetReviewStatsHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ReviewedUserId.Should().Be(reviewedUserId);
        result.TotalReviews.Should().Be(3);
        result.AverageRating.Should().Be(1.0); // All worst ratings

        result.RatingDistribution.Should().HaveCount(5);
        result.RatingDistribution[1].Should().Be(3); // All 3 reviews have rating 1
        result.RatingDistribution[2].Should().Be(0);
        result.RatingDistribution[3].Should().Be(0);
        result.RatingDistribution[4].Should().Be(0);
        result.RatingDistribution[5].Should().Be(0);
    }

    [Fact]
    public async Task Handle_UserWithMixedRatings_CalculatesCorrectAverage()
    {
        // Arrange
        var reviewedUserId = Guid.NewGuid();

        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = "mixed@stats.com",
            PasswordHash = "hashedpassword",
            FullName = "Mixed Stats User",
            Bio = "Gets mixed reviews for statistics",
            Age = 26,
            Gender = "Other",
            University = "Statistics University",
            Lifestyle = "Variable",
            Interests = "Mathematical averages",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        await _dbContext.Profiles.AddAsync(reviewedUser);

        // Specific test case: ratings 2, 3, 4, 5
        // Average should be: (2 + 3 + 4 + 5) / 4 = 14 / 4 = 3.5
        var ratings = new[] { 2, 3, 4, 5 };
        var profiles = new List<Profile>();
        var reviews = new List<Review>();

        for (int i = 0; i < ratings.Length; i++)
        {
            var reviewerId = Guid.NewGuid();
            var rating = ratings[i];

            var reviewer = new Profile
            {
                Id = reviewerId,
                Email = $"mathreviewer{i + 1}@example.com",
                PasswordHash = "hashedpassword",
                FullName = $"Math Reviewer {i + 1}",
                Bio = "Mathematical reviewer",
                Age = 25 + i,
                Gender = "Male",
                University = "Math University",
                Lifestyle = "Logical",
                Interests = "Mathematics",
                Role = "User",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            };

            var review = new Review
            {
                Id = Guid.NewGuid(),
                ReviewerId = reviewerId,
                ReviewedUserId = reviewedUserId,
                Rating = rating,
                Comment = $"Mathematical review with rating {rating}",
                CreatedAt = DateTime.UtcNow.AddDays(-(i + 1))
            };

            profiles.Add(reviewer);
            reviews.Add(review);
        }

        await _dbContext.Profiles.AddRangeAsync(profiles);
        await _dbContext.Reviews.AddRangeAsync(reviews);
        await _dbContext.SaveChangesAsync();

        var request = new GetReviewStatsRequest(reviewedUserId);
        var handler = new GetReviewStatsHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ReviewedUserId.Should().Be(reviewedUserId);
        result.TotalReviews.Should().Be(4);
        result.AverageRating.Should().Be(3.5); // (2+3+4+5)/4 = 3.5

        result.RatingDistribution.Should().HaveCount(5);
        result.RatingDistribution[2].Should().Be(1);
        result.RatingDistribution[3].Should().Be(1);
        result.RatingDistribution[4].Should().Be(1);
        result.RatingDistribution[5].Should().Be(1);
        result.RatingDistribution[1].Should().Be(0);
    }
}
