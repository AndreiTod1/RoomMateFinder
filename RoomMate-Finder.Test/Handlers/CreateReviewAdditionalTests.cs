using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Reviews.CreateReview;
using RoomMate_Finder.Test.Helpers;
using Xunit;

namespace RoomMate_Finder.Test.Handlers;

public class CreateReviewAdditionalTests : IDisposable
{
    [Fact]
    public async Task Given_PositiveReview_When_HandleIsCalled_Then_ReviewIsCreated()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var reviewerId = Guid.NewGuid();
        var reviewedUserId = Guid.NewGuid();

        var reviewer = new Profile
        {
            Id = reviewerId,
            Email = "happy@example.com",
            PasswordHash = "hashedpass",
            FullName = "Happy Reviewer",
            Age = 25,
            Gender = "F",
            University = "Happy University",
            Bio = "Always positive",
            Lifestyle = "Optimistic",
            Interests = "Smiling, Laughing",
            CreatedAt = DateTime.UtcNow
        };

        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = "great@example.com",
            PasswordHash = "hashedpass2",
            FullName = "Great Roommate",
            Age = 26,
            Gender = "M",
            University = "Great University",
            Bio = "Amazing person",
            Lifestyle = "Perfect",
            Interests = "Being awesome",
            CreatedAt = DateTime.UtcNow
        };

        context.Profiles.AddRange(reviewer, reviewedUser);
        await context.SaveChangesAsync();

        var handler = new CreateReviewHandler(context);
        var request = new CreateReviewRequest
        {
            ReviewerId = reviewerId,
            ReviewedUserId = reviewedUserId,
            Rating = 5,
            Comment = "Absolutely fantastic roommate! Clean, respectful, and friendly. Would definitely recommend!"
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Rating.Should().Be(5);
        result.Comment.Should().Contain("fantastic");
        result.ReviewerId.Should().Be(reviewerId);
        result.ReviewedUserId.Should().Be(reviewedUserId);
    }

    [Fact]
    public async Task Given_NegativeReview_When_HandleIsCalled_Then_ReviewIsCreated()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var reviewerId = Guid.NewGuid();
        var reviewedUserId = Guid.NewGuid();

        var reviewer = new Profile
        {
            Id = reviewerId,
            Email = "critic@example.com",
            PasswordHash = "hashedpass",
            FullName = "Critical Reviewer",
            Age = 30,
            Gender = "M",
            University = "Critical University",
            Bio = "Honest feedback",
            Lifestyle = "Realistic",
            Interests = "Truth, Honesty",
            CreatedAt = DateTime.UtcNow
        };

        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = "problematic@example.com",
            PasswordHash = "hashedpass2",
            FullName = "Problematic User",
            Age = 24,
            Gender = "F",
            University = "Problem University",
            Bio = "Has issues",
            Lifestyle = "Messy",
            Interests = "Making noise",
            CreatedAt = DateTime.UtcNow
        };

        context.Profiles.AddRange(reviewer, reviewedUser);
        await context.SaveChangesAsync();

        var handler = new CreateReviewHandler(context);
        var request = new CreateReviewRequest
        {
            ReviewerId = reviewerId,
            ReviewedUserId = reviewedUserId,
            Rating = 2,
            Comment = "Not a great experience. Very messy and loud. Would not room with again."
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Rating.Should().Be(2);
        result.Comment.Should().Contain("messy");
        result.Comment.Should().Contain("loud");
    }

    [Fact]
    public async Task Given_NeutralReview_When_HandleIsCalled_Then_ReviewIsCreated()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var reviewerId = Guid.NewGuid();
        var reviewedUserId = Guid.NewGuid();

        var reviewer = new Profile
        {
            Id = reviewerId,
            Email = "neutral@example.com",
            PasswordHash = "hashedpass",
            FullName = "Neutral Reviewer",
            Age = 27,
            Gender = "F",
            University = "Neutral University",
            Bio = "Balanced perspective",
            Lifestyle = "Balanced",
            Interests = "Objectivity, Fairness",
            CreatedAt = DateTime.UtcNow
        };

        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = "average@example.com",
            PasswordHash = "hashedpass2",
            FullName = "Average User",
            Age = 25,
            Gender = "M",
            University = "Average University",
            Bio = "Nothing special",
            Lifestyle = "Normal",
            Interests = "Regular stuff",
            CreatedAt = DateTime.UtcNow
        };

        context.Profiles.AddRange(reviewer, reviewedUser);
        await context.SaveChangesAsync();

        var handler = new CreateReviewHandler(context);
        var request = new CreateReviewRequest
        {
            ReviewerId = reviewerId,
            ReviewedUserId = reviewedUserId,
            Rating = 3,
            Comment = "Average roommate experience. Nothing particularly good or bad to report."
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Rating.Should().Be(3);
        result.Comment.Should().Contain("Average");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Given_DetailedReview_When_HandleIsCalled_Then_ReviewIsCreated()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var reviewerId = Guid.NewGuid();
        var reviewedUserId = Guid.NewGuid();

        var reviewer = new Profile
        {
            Id = reviewerId,
            Email = "detailed@example.com",
            PasswordHash = "hashedpass",
            FullName = "Detailed Reviewer",
            Age = 29,
            Gender = "M",
            University = "Detailed University",
            Bio = "Thorough in everything",
            Lifestyle = "Meticulous",
            Interests = "Analysis, Details",
            CreatedAt = DateTime.UtcNow
        };

        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = "complex@example.com",
            PasswordHash = "hashedpass2",
            FullName = "Complex User",
            Age = 28,
            Gender = "F",
            University = "Complex University",
            Bio = "Multi-faceted personality",
            Lifestyle = "Complex",
            Interests = "Many different things",
            CreatedAt = DateTime.UtcNow
        };

        context.Profiles.AddRange(reviewer, reviewedUser);
        await context.SaveChangesAsync();

        var handler = new CreateReviewHandler(context);
        var longDetailedComment = "This roommate had both positive and negative aspects. On the positive side, " +
                                  "they were very clean in common areas and always paid rent on time. They were also " +
                                  "respectful of quiet hours and helped with household chores. However, they sometimes " +
                                  "had friends over without much notice, and their cooking smells could be quite strong. " +
                                  "Overall, it was a decent living situation with room for improvement in communication.";
        
        var request = new CreateReviewRequest
        {
            ReviewerId = reviewerId,
            ReviewedUserId = reviewedUserId,
            Rating = 4,
            Comment = longDetailedComment
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Rating.Should().Be(4);
        result.Comment.Should().Be(longDetailedComment);
        result.Comment.Length.Should().BeGreaterThan(200);
    }

    [Fact]
    public async Task Given_ShortReview_When_HandleIsCalled_Then_ReviewIsCreated()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var reviewerId = Guid.NewGuid();
        var reviewedUserId = Guid.NewGuid();

        var reviewer = new Profile
        {
            Id = reviewerId,
            Email = "brief@example.com",
            PasswordHash = "hashedpass",
            FullName = "Brief Reviewer",
            Age = 22,
            Gender = "F",
            University = "Brief University",
            Bio = "Keep it short",
            Lifestyle = "Concise",
            Interests = "Brevity",
            CreatedAt = DateTime.UtcNow
        };

        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = "simple@example.com",
            PasswordHash = "hashedpass2",
            FullName = "Simple User",
            Age = 23,
            Gender = "M",
            University = "Simple University",
            Bio = "Keep it simple",
            Lifestyle = "Simple",
            Interests = "Simple things",
            CreatedAt = DateTime.UtcNow
        };

        context.Profiles.AddRange(reviewer, reviewedUser);
        await context.SaveChangesAsync();

        var handler = new CreateReviewHandler(context);
        var request = new CreateReviewRequest
        {
            ReviewerId = reviewerId,
            ReviewedUserId = reviewedUserId,
            Rating = 4,
            Comment = "Good roommate!"
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Rating.Should().Be(4);
        result.Comment.Should().Be("Good roommate!");
        result.Comment.Length.Should().BeLessThan(20);
    }

    public void Dispose()
    {
        // Clean up resources if needed
    }
}
