using FluentAssertions;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Reviews.GetUserReviews;
using RoomMate_Finder.Infrastructure.Persistence;
using RoomMate_Finder.Test.Helpers;

namespace RoomMate_Finder.Test.Handlers.Reviews;

public class GetUserReviewsHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;

    public GetUserReviewsHandlerTests()
    {
        _dbContext = DbContextHelper.CreateInMemoryDbContext();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_UserWithReviews_ReturnsAllReviews()
    {
        // Arrange
        var reviewedUserId = Guid.NewGuid();
        var reviewer1Id = Guid.NewGuid();
        var reviewer2Id = Guid.NewGuid();

        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = "reviewed@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Popular Reviewee",
            Bio = "Gets lots of reviews",
            Age = 24,
            Gender = "Female",
            University = "Review University",
            Lifestyle = "Social",
            Interests = "Meeting people",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        var reviewer1 = new Profile
        {
            Id = reviewer1Id,
            Email = "reviewer1@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Alice Reviewer",
            Bio = "First reviewer",
            Age = 25,
            Gender = "Female",
            University = "Review University",
            Lifestyle = "Active",
            Interests = "Reviewing",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-90)
        };

        var reviewer2 = new Profile
        {
            Id = reviewer2Id,
            Email = "reviewer2@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Bob Reviewer",
            Bio = "Second reviewer",
            Age = 26,
            Gender = "Male",
            University = "Review University",
            Lifestyle = "Calm",
            Interests = "Writing reviews",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-80)
        };

        var review1 = new Review
        {
            Id = Guid.NewGuid(),
            ReviewerId = reviewer1Id,
            ReviewedUserId = reviewedUserId,
            Rating = 5,
            Comment = "Excellent roommate! Very clean and organized.",
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        var review2 = new Review
        {
            Id = Guid.NewGuid(),
            ReviewerId = reviewer2Id,
            ReviewedUserId = reviewedUserId,
            Rating = 4,
            Comment = "Good roommate, friendly and respectful.",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        await _dbContext.Profiles.AddRangeAsync(reviewedUser, reviewer1, reviewer2);
        await _dbContext.Reviews.AddRangeAsync(review1, review2);
        await _dbContext.SaveChangesAsync();

        var request = new GetUserReviewsRequest(reviewedUserId);
        var handler = new GetUserReviewsHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Reviews.Should().HaveCount(2);

        var reviewsList = result.Reviews.ToList();
        
        // Check first review (most recent should be first)
        var firstReview = reviewsList.First();
        firstReview.ReviewerId.Should().Be(reviewer2Id);
        firstReview.ReviewerFullName.Should().Be("Bob Reviewer");
        firstReview.Rating.Should().Be(4);
        firstReview.Comment.Should().Be("Good roommate, friendly and respectful.");

        // Check second review
        var secondReview = reviewsList.Last();
        secondReview.ReviewerId.Should().Be(reviewer1Id);
        secondReview.ReviewerFullName.Should().Be("Alice Reviewer");
        secondReview.Rating.Should().Be(5);
        secondReview.Comment.Should().Be("Excellent roommate! Very clean and organized.");

        // Verify all reviews are for the correct user
        reviewsList.Should().AllSatisfy(r => r.Id.Should().NotBeEmpty());
        reviewsList.Should().AllSatisfy(r => r.CreatedAt.Should().BeAfter(DateTime.UtcNow.AddDays(-15)));
    }

    [Fact]
    public async Task Handle_UserWithoutReviews_ReturnsEmptyCollection()
    {
        // Arrange
        var userWithoutReviewsId = Guid.NewGuid();

        var userWithoutReviews = new Profile
        {
            Id = userWithoutReviewsId,
            Email = "lonely@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Lonely User",
            Bio = "No reviews yet",
            Age = 22,
            Gender = "Male",
            University = "University",
            Lifestyle = "Quiet",
            Interests = "Waiting for reviews",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        await _dbContext.Profiles.AddAsync(userWithoutReviews);
        await _dbContext.SaveChangesAsync();

        var request = new GetUserReviewsRequest(userWithoutReviewsId);
        var handler = new GetUserReviewsHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Reviews.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NonExistentUser_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();

        var request = new GetUserReviewsRequest(nonExistentUserId);
        var handler = new GetUserReviewsHandler(_dbContext);

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found");
    }

    [Fact]
    public async Task Handle_UserWithManyReviews_ReturnsAllReviewsSortedByDate()
    {
        // Arrange
        var reviewedUserId = Guid.NewGuid();
        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = "popular@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Very Popular User",
            Bio = "Gets many reviews",
            Age = 25,
            Gender = "Other",
            University = "University",
            Lifestyle = "Social",
            Interests = "Being reviewed",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-100)
        };

        await _dbContext.Profiles.AddAsync(reviewedUser);

        var reviews = new List<Review>();
        var profiles = new List<Profile>();

        for (int i = 1; i <= 5; i++)
        {
            var reviewerId = Guid.NewGuid();
            var reviewer = new Profile
            {
                Id = reviewerId,
                Email = $"reviewer{i}@example.com",
                PasswordHash = "hashedpassword",
                FullName = $"Reviewer {i}",
                Bio = $"Reviewer number {i}",
                Age = 20 + i,
                Gender = i % 2 == 0 ? "Female" : "Male",
                University = "University",
                Lifestyle = "Active",
                Interests = "Reviewing",
                Role = "User",
                CreatedAt = DateTime.UtcNow.AddDays(-50)
            };

            var review = new Review
            {
                Id = Guid.NewGuid(),
                ReviewerId = reviewerId,
                ReviewedUserId = reviewedUserId,
                Rating = i, // Ratings 1-5
                Comment = $"Review number {i} with rating {i}",
                CreatedAt = DateTime.UtcNow.AddDays(-i) // Most recent first
            };

            profiles.Add(reviewer);
            reviews.Add(review);
        }

        await _dbContext.Profiles.AddRangeAsync(profiles);
        await _dbContext.Reviews.AddRangeAsync(reviews);
        await _dbContext.SaveChangesAsync();

        var request = new GetUserReviewsRequest(reviewedUserId);
        var handler = new GetUserReviewsHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Reviews.Should().HaveCount(5);

        var reviewsList = result.Reviews.ToList();
        
        // Should be ordered by CreatedAt descending (most recent first)
        reviewsList[0].Rating.Should().Be(1); // Most recent (AddDays(-1))
        reviewsList[1].Rating.Should().Be(2); // AddDays(-2)
        reviewsList[2].Rating.Should().Be(3); // AddDays(-3)
        reviewsList[3].Rating.Should().Be(4); // AddDays(-4)
        reviewsList[4].Rating.Should().Be(5); // AddDays(-5)

        // Verify all reviews contain correct data
        for (int i = 0; i < reviewsList.Count; i++)
        {
            var review = reviewsList[i];
            var expectedRating = i + 1;
            review.Rating.Should().Be(expectedRating);
            review.Comment.Should().Be($"Review number {expectedRating} with rating {expectedRating}");
            review.ReviewerFullName.Should().Be($"Reviewer {expectedRating}");
        }
    }

    [Fact]
    public async Task Handle_UserWithMixedRatings_ReturnsCorrectAverageImpliedByData()
    {
        // Arrange
        var reviewedUserId = Guid.NewGuid();

        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = "mixed@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Mixed Ratings User",
            Bio = "Gets mixed reviews",
            Age = 25,
            Gender = "Female",
            University = "University",
            Lifestyle = "Variable",
            Interests = "Inconsistency",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        await _dbContext.Profiles.AddAsync(reviewedUser);

        var reviewData = new[]
        {
            new { Rating = 1, Comment = "Poor experience", ReviewerName = "Harsh Reviewer" },
            new { Rating = 5, Comment = "Excellent roommate", ReviewerName = "Happy Reviewer" },
            new { Rating = 3, Comment = "Average experience", ReviewerName = "Neutral Reviewer" },
            new { Rating = 4, Comment = "Good overall", ReviewerName = "Positive Reviewer" },
            new { Rating = 2, Comment = "Below expectations", ReviewerName = "Critical Reviewer" }
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
                Email = $"reviewer{i + 1}@mixed.com",
                PasswordHash = "hashedpassword",
                FullName = data.ReviewerName,
                Bio = "Mixed reviewer",
                Age = 25 + i,
                Gender = "Male",
                University = "University",
                Lifestyle = "Active",
                Interests = "Honest reviews",
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

        var request = new GetUserReviewsRequest(reviewedUserId);
        var handler = new GetUserReviewsHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Reviews.Should().HaveCount(5);

        var reviewsList = result.Reviews.ToList();

        // Verify we have all expected ratings
        var ratings = reviewsList.Select(r => r.Rating).OrderBy(r => r).ToArray();
        ratings.Should().Equal(new[] { 1, 2, 3, 4, 5 });

        // Verify comments match ratings
        var poorReview = reviewsList.First(r => r.Rating == 1);
        poorReview.Comment.Should().Be("Poor experience");
        poorReview.ReviewerFullName.Should().Be("Harsh Reviewer");

        var excellentReview = reviewsList.First(r => r.Rating == 5);
        excellentReview.Comment.Should().Be("Excellent roommate");
        excellentReview.ReviewerFullName.Should().Be("Happy Reviewer");

        // Verify ordering (most recent first)
        var orderedByDate = reviewsList.OrderByDescending(r => r.CreatedAt).ToList();
        for (int i = 0; i < reviewsList.Count; i++)
        {
            reviewsList[i].Id.Should().Be(orderedByDate[i].Id);
        }
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ThrowsKeyNotFoundException()
    {
        // Arrange
        var randomUserId = Guid.NewGuid();

        var request = new GetUserReviewsRequest(randomUserId);
        var handler = new GetUserReviewsHandler(_dbContext);

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found");
    }
}
