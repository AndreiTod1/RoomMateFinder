using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Reviews.CreateReview;
using RoomMate_Finder.Features.Reviews.GetUserReviews;
using RoomMate_Finder.Features.Reviews.GetReviewStats;
using RoomMate_Finder.Test.Helpers;

namespace RoomMate_Finder.Test.Handlers;

#region Create Review Handler Tests

public class CreateReviewHandlerTests
{
    private static Profile CreateTestProfile(Guid? id = null, string name = "Test User")
    {
        return new Profile
        {
            Id = id ?? Guid.NewGuid(),
            Email = $"test{Guid.NewGuid():N}@test.com",
            PasswordHash = "hashed",
            FullName = name,
            Age = 25,
            Gender = "Male",
            University = "Test Uni",
            Bio = "Bio",
            Lifestyle = "quiet",
            Interests = "music",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Given_ReviewedUserNotFound_When_HandleIsCalled_Then_ThrowsKeyNotFoundException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var reviewer = CreateTestProfile(name: "Reviewer");
        context.Profiles.Add(reviewer);
        await context.SaveChangesAsync();

        var handler = new CreateReviewHandler(context);
        var request = new CreateReviewRequest
        {
            ReviewerId = reviewer.Id,
            ReviewedUserId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Great!"
        };

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Given_UserReviewsThemselves_When_HandleIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user = CreateTestProfile(name: "User");
        context.Profiles.Add(user);
        await context.SaveChangesAsync();

        var handler = new CreateReviewHandler(context);
        var request = new CreateReviewRequest
        {
            ReviewerId = user.Id,
            ReviewedUserId = user.Id,
            Rating = 5,
            Comment = "Great!"
        };

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*yourself*");
    }

    [Fact]
    public async Task Given_DuplicateReview_When_HandleIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var reviewer = CreateTestProfile(name: "Reviewer");
        var reviewed = CreateTestProfile(name: "Reviewed");
        context.Profiles.AddRange(reviewer, reviewed);
        
        context.Reviews.Add(new Review
        {
            Id = Guid.NewGuid(),
            ReviewerId = reviewer.Id,
            ReviewedUserId = reviewed.Id,
            Rating = 4,
            Comment = "Good",
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var handler = new CreateReviewHandler(context);
        var request = new CreateReviewRequest
        {
            ReviewerId = reviewer.Id,
            ReviewedUserId = reviewed.Id,
            Rating = 5,
            Comment = "Great!"
        };

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Given_ValidRequest_When_HandleIsCalled_Then_CreatesReview()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var reviewer = CreateTestProfile(name: "Reviewer");
        var reviewed = CreateTestProfile(name: "Reviewed");
        context.Profiles.AddRange(reviewer, reviewed);
        await context.SaveChangesAsync();

        var handler = new CreateReviewHandler(context);
        var request = new CreateReviewRequest
        {
            ReviewerId = reviewer.Id,
            ReviewedUserId = reviewed.Id,
            Rating = 5,
            Comment = "Excellent roommate!"
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Rating.Should().Be(5);
        result.Comment.Should().Be("Excellent roommate!");
        
        var savedReview = await context.Reviews.FirstOrDefaultAsync();
        savedReview.Should().NotBeNull();
    }
}

#endregion

#region Get User Reviews Handler Tests

public class GetUserReviewsHandlerTests
{
    private static Profile CreateTestProfile(Guid? id = null, string name = "Test User")
    {
        return new Profile
        {
            Id = id ?? Guid.NewGuid(),
            Email = $"test{Guid.NewGuid():N}@test.com",
            PasswordHash = "hashed",
            FullName = name,
            Age = 25,
            Gender = "Male",
            University = "Test Uni",
            Bio = "Bio",
            Lifestyle = "quiet",
            Interests = "music",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Given_NoReviews_When_HandleIsCalled_Then_ReturnsEmptyList()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user = CreateTestProfile();
        context.Profiles.Add(user);
        await context.SaveChangesAsync();

        var handler = new GetUserReviewsHandler(context);
        var request = new GetUserReviewsRequest(user.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Reviews.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_ReviewsExist_When_HandleIsCalled_Then_ReturnsUserReviews()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var reviewed = CreateTestProfile(name: "Reviewed");
        var reviewer1 = CreateTestProfile(name: "Reviewer 1");
        var reviewer2 = CreateTestProfile(name: "Reviewer 2");
        context.Profiles.AddRange(reviewed, reviewer1, reviewer2);

        context.Reviews.AddRange(
            new Review { Id = Guid.NewGuid(), ReviewerId = reviewer1.Id, Reviewer = reviewer1, ReviewedUserId = reviewed.Id, Rating = 5, Comment = "Great!", CreatedAt = DateTime.UtcNow },
            new Review { Id = Guid.NewGuid(), ReviewerId = reviewer2.Id, Reviewer = reviewer2, ReviewedUserId = reviewed.Id, Rating = 4, Comment = "Good!", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var handler = new GetUserReviewsHandler(context);
        var request = new GetUserReviewsRequest(reviewed.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Reviews.Should().HaveCount(2);
    }
}

#endregion

#region Get Review Stats Handler Tests

public class GetReviewStatsHandlerTests
{
    private static Profile CreateTestProfile(Guid? id = null, string name = "Test User")
    {
        return new Profile
        {
            Id = id ?? Guid.NewGuid(),
            Email = $"test{Guid.NewGuid():N}@test.com",
            PasswordHash = "hashed",
            FullName = name,
            Age = 25,
            Gender = "Male",
            University = "Test Uni",
            Bio = "Bio",
            Lifestyle = "quiet",
            Interests = "music",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Given_NoReviews_When_HandleIsCalled_Then_ReturnsZeroStats()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user = CreateTestProfile();
        context.Profiles.Add(user);
        await context.SaveChangesAsync();

        var handler = new GetReviewStatsHandler(context);
        var request = new GetReviewStatsRequest(user.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.TotalReviews.Should().Be(0);
        result.AverageRating.Should().Be(0);
    }

    [Fact]
    public async Task Given_ReviewsExist_When_HandleIsCalled_Then_ReturnsCorrectStats()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var reviewed = CreateTestProfile(name: "Reviewed");
        var reviewer1 = CreateTestProfile(name: "Reviewer 1");
        var reviewer2 = CreateTestProfile(name: "Reviewer 2");
        context.Profiles.AddRange(reviewed, reviewer1, reviewer2);

        context.Reviews.AddRange(
            new Review { Id = Guid.NewGuid(), ReviewerId = reviewer1.Id, ReviewedUserId = reviewed.Id, Rating = 5, Comment = "Great!", CreatedAt = DateTime.UtcNow },
            new Review { Id = Guid.NewGuid(), ReviewerId = reviewer2.Id, ReviewedUserId = reviewed.Id, Rating = 3, Comment = "OK", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var handler = new GetReviewStatsHandler(context);
        var request = new GetReviewStatsRequest(reviewed.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.TotalReviews.Should().Be(2);
        result.AverageRating.Should().Be(4.0); // (5 + 3) / 2 = 4
    }
}

#endregion
