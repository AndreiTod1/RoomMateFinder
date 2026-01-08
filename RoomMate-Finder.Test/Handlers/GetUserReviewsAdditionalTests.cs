using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Reviews.GetUserReviews;
using RoomMate_Finder.Test.Helpers;
using Xunit;

namespace RoomMate_Finder.Test.Handlers;

public class GetUserReviewsAdditionalTests : IDisposable
{
    [Fact]
    public async Task Given_UserWithMixedRatings_When_HandleIsCalled_Then_ReturnsAllReviews()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var reviewedUserId = Guid.NewGuid();
        var reviewer1Id = Guid.NewGuid();
        var reviewer2Id = Guid.NewGuid();
        var reviewer3Id = Guid.NewGuid();

        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = "mixed@example.com",
            PasswordHash = "hashedpass",
            FullName = "Mixed Reviews User",
            Age = 25,
            Gender = "M",
            University = "Mixed University",
            Bio = "Gets mixed reviews",
            Lifestyle = "Variable",
            Interests = "Inconsistent",
            CreatedAt = DateTime.UtcNow
        };

        var reviewer1 = new Profile
        {
            Id = reviewer1Id,
            Email = "lover@example.com",
            PasswordHash = "hashedpass1",
            FullName = "Love Him",
            Age = 26,
            Gender = "F",
            University = "Positive University",
            Bio = "Always positive",
            Lifestyle = "Optimistic",
            Interests = "Good vibes",
            CreatedAt = DateTime.UtcNow
        };

        var reviewer2 = new Profile
        {
            Id = reviewer2Id,
            Email = "hater@example.com",
            PasswordHash = "hashedpass2",
            FullName = "Not A Fan",
            Age = 24,
            Gender = "F",
            University = "Negative University",
            Bio = "Critical person",
            Lifestyle = "Pessimistic",
            Interests = "Complaining",
            CreatedAt = DateTime.UtcNow
        };

        var reviewer3 = new Profile
        {
            Id = reviewer3Id,
            Email = "neutral@example.com",
            PasswordHash = "hashedpass3",
            FullName = "Neutral Person",
            Age = 27,
            Gender = "M",
            University = "Neutral University",
            Bio = "Balanced view",
            Lifestyle = "Moderate",
            Interests = "Being fair",
            CreatedAt = DateTime.UtcNow
        };

        var review1 = new Review
        {
            Id = Guid.NewGuid(),
            ReviewerId = reviewer1Id,
            ReviewedUserId = reviewedUserId,
            Rating = 5,
            Comment = "Amazing roommate! Best experience ever!",
            CreatedAt = DateTime.UtcNow
        };

        var review2 = new Review
        {
            Id = Guid.NewGuid(),
            ReviewerId = reviewer2Id,
            ReviewedUserId = reviewedUserId,
            Rating = 1,
            Comment = "Terrible experience. Worst roommate ever.",
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        var review3 = new Review
        {
            Id = Guid.NewGuid(),
            ReviewerId = reviewer3Id,
            ReviewedUserId = reviewedUserId,
            Rating = 3,
            Comment = "Average roommate. Nothing special.",
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        context.Profiles.AddRange(reviewedUser, reviewer1, reviewer2, reviewer3);
        context.Reviews.AddRange(review1, review2, review3);
        await context.SaveChangesAsync();

        var handler = new GetUserReviewsHandler(context);
        var request = new GetUserReviewsRequest(reviewedUserId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Reviews.Should().HaveCount(3);
        
        var reviewsList = result.Reviews.ToList();
        reviewsList.Should().Contain(r => r.Rating == 5);
        reviewsList.Should().Contain(r => r.Rating == 1);
        reviewsList.Should().Contain(r => r.Rating == 3);
    }

    [Fact]
    public async Task Given_UserWithOnlyPositiveReviews_When_HandleIsCalled_Then_ReturnsAllPositive()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var reviewedUserId = Guid.NewGuid();
        var reviewer1Id = Guid.NewGuid();
        var reviewer2Id = Guid.NewGuid();

        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = "perfect@example.com",
            PasswordHash = "hashedpass",
            FullName = "Perfect Roommate",
            Age = 26,
            Gender = "F",
            University = "Perfect University",
            Bio = "Everyone loves me",
            Lifestyle = "Ideal",
            Interests = "Being perfect",
            CreatedAt = DateTime.UtcNow
        };

        var reviewer1 = new Profile
        {
            Id = reviewer1Id,
            Email = "fan1@example.com",
            PasswordHash = "hashedpass1",
            FullName = "Big Fan One",
            Age = 25,
            Gender = "M",
            University = "Fan University",
            Bio = "Love great roommates",
            Lifestyle = "Appreciative",
            Interests = "Gratitude",
            CreatedAt = DateTime.UtcNow
        };

        var reviewer2 = new Profile
        {
            Id = reviewer2Id,
            Email = "fan2@example.com",
            PasswordHash = "hashedpass2",
            FullName = "Big Fan Two",
            Age = 27,
            Gender = "M",
            University = "Fan University",
            Bio = "Also love great roommates",
            Lifestyle = "Thankful",
            Interests = "Positivity",
            CreatedAt = DateTime.UtcNow
        };

        var review1 = new Review
        {
            Id = Guid.NewGuid(),
            ReviewerId = reviewer1Id,
            ReviewedUserId = reviewedUserId,
            Rating = 5,
            Comment = "Absolutely perfect in every way!",
            CreatedAt = DateTime.UtcNow
        };

        var review2 = new Review
        {
            Id = Guid.NewGuid(),
            ReviewerId = reviewer2Id,
            ReviewedUserId = reviewedUserId,
            Rating = 5,
            Comment = "Could not ask for a better roommate!",
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        context.Profiles.AddRange(reviewedUser, reviewer1, reviewer2);
        context.Reviews.AddRange(review1, review2);
        await context.SaveChangesAsync();

        var handler = new GetUserReviewsHandler(context);
        var request = new GetUserReviewsRequest(reviewedUserId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Reviews.Should().HaveCount(2);
        result.Reviews.Should().OnlyContain(r => r.Rating == 5);
        result.Reviews.Should().OnlyContain(r => r.Comment.Contains("perfect") || r.Comment.Contains("better"));
    }

    [Fact]
    public async Task Given_UserWithOldReviews_When_HandleIsCalled_Then_ReturnsAllReviewsOrderedByDate()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var reviewedUserId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();

        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = "longterm@example.com",
            PasswordHash = "hashedpass",
            FullName = "Long Term User",
            Age = 30,
            Gender = "M",
            University = "Long University",
            Bio = "Been around a while",
            Lifestyle = "Established",
            Interests = "History",
            CreatedAt = DateTime.UtcNow.AddYears(-2)
        };

        var reviewer = new Profile
        {
            Id = reviewerId,
            Email = "oldreviewer@example.com",
            PasswordHash = "hashedpass1",
            FullName = "Old Reviewer",
            Age = 35,
            Gender = "F",
            University = "Old University",
            Bio = "Been reviewing for years",
            Lifestyle = "Experienced",
            Interests = "Long-term perspective",
            CreatedAt = DateTime.UtcNow.AddYears(-3)
        };

        var oldReview = new Review
        {
            Id = Guid.NewGuid(),
            ReviewerId = reviewerId,
            ReviewedUserId = reviewedUserId,
            Rating = 4,
            Comment = "Good roommate from way back when.",
            CreatedAt = DateTime.UtcNow.AddYears(-1)
        };

        context.Profiles.AddRange(reviewedUser, reviewer);
        context.Reviews.Add(oldReview);
        await context.SaveChangesAsync();

        var handler = new GetUserReviewsHandler(context);
        var request = new GetUserReviewsRequest(reviewedUserId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Reviews.Should().HaveCount(1);
        result.Reviews.First().Comment.Should().Contain("way back when");
        result.Reviews.First().ReviewerFullName.Should().Be("Old Reviewer");
    }

    public void Dispose()
    {
        // Clean up resources if needed
    }
}
