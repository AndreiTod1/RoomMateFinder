using FluentAssertions;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Reviews.CreateReview;
using RoomMate_Finder.Infrastructure.Persistence;
using RoomMate_Finder.Test.Helpers;

namespace RoomMate_Finder.Test.Handlers.Reviews;

public class CreateReviewHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;

    public CreateReviewHandlerTests()
    {
        _dbContext = DbContextHelper.CreateInMemoryDbContext();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_ValidReview_CreatesReviewSuccessfully()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();
        var reviewedUserId = Guid.NewGuid();

        var reviewer = new Profile
        {
            Id = reviewerId,
            Email = "reviewer@example.com",
            PasswordHash = "hashedpassword",
            FullName = "John Reviewer",
            Bio = "I write honest reviews",
            Age = 25,
            Gender = "Male",
            University = "Review University",
            Lifestyle = "Active",
            Interests = "Reviewing, Feedback",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = "reviewed@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Jane Reviewee",
            Bio = "Good roommate candidate",
            Age = 23,
            Gender = "Female",
            University = "Review University",
            Lifestyle = "Calm",
            Interests = "Reading, Music",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        await _dbContext.Profiles.AddRangeAsync(reviewer, reviewedUser);
        await _dbContext.SaveChangesAsync();

        var request = new CreateReviewRequest
        {
            ReviewerId = reviewerId,
            ReviewedUserId = reviewedUserId,
            Rating = 5,
            Comment = "Excellent roommate! Very clean and respectful."
        };

        var handler = new CreateReviewHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.ReviewerId.Should().Be(reviewerId);
        result.ReviewedUserId.Should().Be(reviewedUserId);
        result.Rating.Should().Be(5);
        result.Comment.Should().Be("Excellent roommate! Very clean and respectful.");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        var createdReview = await _dbContext.Reviews.FindAsync(result.Id);
        createdReview.Should().NotBeNull();
        createdReview!.ReviewerId.Should().Be(reviewerId);
        createdReview.ReviewedUserId.Should().Be(reviewedUserId);
        createdReview.Rating.Should().Be(5);
        createdReview.Comment.Should().Be("Excellent roommate! Very clean and respectful.");
    }

    [Fact]
    public async Task Handle_NonExistentReviewedUser_ThrowsKeyNotFoundException()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();
        var nonExistentUserId = Guid.NewGuid();

        var reviewer = new Profile
        {
            Id = reviewerId,
            Email = "reviewer@example.com",
            PasswordHash = "hashedpassword",
            FullName = "John Reviewer",
            Bio = "I write reviews",
            Age = 25,
            Gender = "Male",
            University = "University",
            Lifestyle = "Active",
            Interests = "Reviews",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        await _dbContext.Profiles.AddAsync(reviewer);
        await _dbContext.SaveChangesAsync();

        var request = new CreateReviewRequest
        {
            ReviewerId = reviewerId,
            ReviewedUserId = nonExistentUserId,
            Rating = 4,
            Comment = "This user doesn't exist"
        };

        var handler = new CreateReviewHandler(_dbContext);

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Reviewed user not found*");
    }

    [Fact]
    public async Task Handle_SelfReview_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var user = new Profile
        {
            Id = userId,
            Email = "selfreviewer@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Self Reviewer",
            Bio = "Trying to review myself",
            Age = 25,
            Gender = "Male",
            University = "University",
            Lifestyle = "Active",
            Interests = "Self-review",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        await _dbContext.Profiles.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        var request = new CreateReviewRequest
        {
            ReviewerId = userId,
            ReviewedUserId = userId, // Same user trying to review themselves
            Rating = 5,
            Comment = "I'm amazing!"
        };

        var handler = new CreateReviewHandler(_dbContext);

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cannot review yourself*");
    }

    [Fact]
    public async Task Handle_DuplicateReview_ThrowsInvalidOperationException()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();
        var reviewedUserId = Guid.NewGuid();

        var reviewer = new Profile
        {
            Id = reviewerId,
            Email = "reviewer@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Duplicate Reviewer",
            Bio = "Trying to review twice",
            Age = 25,
            Gender = "Male",
            University = "University",
            Lifestyle = "Active",
            Interests = "Reviews",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = "reviewed@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Twice Reviewed",
            Bio = "Getting reviewed multiple times",
            Age = 23,
            Gender = "Female",
            University = "University",
            Lifestyle = "Calm",
            Interests = "Being reviewed",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        var existingReview = new Review
        {
            Id = Guid.NewGuid(),
            ReviewerId = reviewerId,
            ReviewedUserId = reviewedUserId,
            Rating = 4,
            Comment = "First review",
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        await _dbContext.Profiles.AddRangeAsync(reviewer, reviewedUser);
        await _dbContext.Reviews.AddAsync(existingReview);
        await _dbContext.SaveChangesAsync();

        var request = new CreateReviewRequest
        {
            ReviewerId = reviewerId,
            ReviewedUserId = reviewedUserId,
            Rating = 5,
            Comment = "Second review attempt"
        };

        var handler = new CreateReviewHandler(_dbContext);

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Review already exists from this user*");
    }

    [Theory]
    [InlineData(1, "Poor roommate experience")]
    [InlineData(2, "Below average")]
    [InlineData(3, "Average roommate")]
    [InlineData(4, "Good roommate")]
    [InlineData(5, "Excellent roommate")]
    public async Task Handle_DifferentRatings_CreatesReviewWithCorrectRating(int rating, string comment)
    {
        // Arrange
        var reviewerId = Guid.NewGuid();
        var reviewedUserId = Guid.NewGuid();

        var reviewer = new Profile
        {
            Id = reviewerId,
            Email = $"reviewer{rating}@example.com",
            PasswordHash = "hashedpassword",
            FullName = $"Reviewer {rating}",
            Bio = "Rating tester",
            Age = 25,
            Gender = "Male",
            University = "University",
            Lifestyle = "Active",
            Interests = "Testing",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = $"reviewed{rating}@example.com",
            PasswordHash = "hashedpassword",
            FullName = $"Reviewed User {rating}",
            Bio = "Being tested",
            Age = 23,
            Gender = "Female",
            University = "University",
            Lifestyle = "Calm",
            Interests = "Being reviewed",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        await _dbContext.Profiles.AddRangeAsync(reviewer, reviewedUser);
        await _dbContext.SaveChangesAsync();

        var request = new CreateReviewRequest
        {
            ReviewerId = reviewerId,
            ReviewedUserId = reviewedUserId,
            Rating = rating,
            Comment = comment
        };

        var handler = new CreateReviewHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Rating.Should().Be(rating);
        result.Comment.Should().Be(comment);

        var createdReview = await _dbContext.Reviews.FindAsync(result.Id);
        createdReview.Should().NotBeNull();
        createdReview!.Rating.Should().Be(rating);
        createdReview.Comment.Should().Be(comment);
    }

    [Fact]
    public async Task Handle_EmptyComment_CreatesReviewSuccessfully()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();
        var reviewedUserId = Guid.NewGuid();

        var reviewer = new Profile
        {
            Id = reviewerId,
            Email = "quietreviewer@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Quiet Reviewer",
            Bio = "Prefers not to comment",
            Age = 25,
            Gender = "Male",
            University = "University",
            Lifestyle = "Active",
            Interests = "Silent reviews",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = "reviewed@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Reviewed User",
            Bio = "Being reviewed silently",
            Age = 23,
            Gender = "Female",
            University = "University",
            Lifestyle = "Calm",
            Interests = "Quiet spaces",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        await _dbContext.Profiles.AddRangeAsync(reviewer, reviewedUser);
        await _dbContext.SaveChangesAsync();

        var request = new CreateReviewRequest
        {
            ReviewerId = reviewerId,
            ReviewedUserId = reviewedUserId,
            Rating = 4,
            Comment = string.Empty // Empty comment
        };

        var handler = new CreateReviewHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Rating.Should().Be(4);
        result.Comment.Should().BeEmpty();

        var createdReview = await _dbContext.Reviews.FindAsync(result.Id);
        createdReview.Should().NotBeNull();
        createdReview!.Rating.Should().Be(4);
        createdReview.Comment.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_LongComment_CreatesReviewSuccessfully()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();
        var reviewedUserId = Guid.NewGuid();

        var reviewer = new Profile
        {
            Id = reviewerId,
            Email = "detailedreviewer@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Detailed Reviewer",
            Bio = "Writes comprehensive reviews",
            Age = 25,
            Gender = "Male",
            University = "University",
            Lifestyle = "Active",
            Interests = "Detailed feedback",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var reviewedUser = new Profile
        {
            Id = reviewedUserId,
            Email = "reviewed@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Thoroughly Reviewed",
            Bio = "Getting detailed review",
            Age = 23,
            Gender = "Female",
            University = "University",
            Lifestyle = "Calm",
            Interests = "Comprehensive feedback",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        await _dbContext.Profiles.AddRangeAsync(reviewer, reviewedUser);
        await _dbContext.SaveChangesAsync();

        var longComment = "This is a very detailed review about my roommate experience. " +
                         "They were incredibly clean, always respectful of shared spaces, " +
                         "and maintained excellent communication throughout our time living together. " +
                         "I would highly recommend them to anyone looking for a responsible roommate. " +
                         "Their habits align well with maintaining a peaceful living environment.";

        var request = new CreateReviewRequest
        {
            ReviewerId = reviewerId,
            ReviewedUserId = reviewedUserId,
            Rating = 5,
            Comment = longComment
        };

        var handler = new CreateReviewHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Rating.Should().Be(5);
        result.Comment.Should().Be(longComment);

        var createdReview = await _dbContext.Reviews.FindAsync(result.Id);
        createdReview.Should().NotBeNull();
        createdReview!.Comment.Should().Be(longComment);
    }
}
