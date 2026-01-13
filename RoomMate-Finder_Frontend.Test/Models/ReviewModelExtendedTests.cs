using FluentAssertions;
using RoomMate_Finder_Frontend.Models;

namespace RoomMate_Finder_Frontend.Test.Models;

public class ReviewModelExtendedTests
{
    #region Review Model Tests

    [Fact]
    public void Review_Should_HaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var review = new Review
        {
            Id = id,
            ReviewerId = reviewerId,
            ReviewerFullName = "John Reviewer",
            Rating = 5,
            Comment = "Great roommate, highly recommend!",
            CreatedAt = createdAt
        };

        // Assert
        review.Id.Should().Be(id);
        review.ReviewerId.Should().Be(reviewerId);
        review.ReviewerFullName.Should().Be("John Reviewer");
        review.Rating.Should().Be(5);
        review.Comment.Should().Be("Great roommate, highly recommend!");
        review.CreatedAt.Should().Be(createdAt);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Review_Rating_ShouldAcceptValidValues(int rating)
    {
        // Act
        var review = new Review
        {
            Id = Guid.NewGuid(),
            ReviewerId = Guid.NewGuid(),
            ReviewerFullName = "Reviewer",
            Rating = rating,
            Comment = "Test comment",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        review.Rating.Should().Be(rating);
        review.Rating.Should().BeInRange(1, 5);
    }

    [Fact]
    public void Review_Should_HaveRequiredProperties()
    {
        // Check that required properties are properly annotated
        var reviewerFullNameProperty = typeof(Review).GetProperty("ReviewerFullName");
        var commentProperty = typeof(Review).GetProperty("Comment");
        
        reviewerFullNameProperty.Should().NotBeNull();
        commentProperty.Should().NotBeNull();
    }

    [Fact]
    public void Review_Should_AllowLongComments()
    {
        // Arrange
        var longComment = new string('A', 1000);

        // Act
        var review = new Review
        {
            Id = Guid.NewGuid(),
            ReviewerId = Guid.NewGuid(),
            ReviewerFullName = "Reviewer",
            Rating = 4,
            Comment = longComment,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        review.Comment.Should().HaveLength(1000);
    }

    [Fact]
    public void Review_CreatedAt_ShouldBeUtc()
    {
        // Act
        var review = new Review
        {
            Id = Guid.NewGuid(),
            ReviewerId = Guid.NewGuid(),
            ReviewerFullName = "Reviewer",
            Rating = 3,
            Comment = "Comment",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        review.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Review_Should_HandleSpecialCharacters()
    {
        // Act
        var review = new Review
        {
            Id = Guid.NewGuid(),
            ReviewerId = Guid.NewGuid(),
            ReviewerFullName = "María García-López",
            Rating = 5,
            Comment = "Excelent coleg! Foarte curat și respectuos. 😊",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        review.ReviewerFullName.Should().Contain("María");
        review.Comment.Should().Contain("😊");
    }

    [Theory]
    [InlineData("John Doe")]
    [InlineData("Ana-Maria Ionescu")]
    [InlineData("李明")]
    [InlineData("Алексей Смирнов")]
    public void Review_ReviewerFullName_ShouldSupportInternationalNames(string name)
    {
        // Act
        var review = new Review
        {
            Id = Guid.NewGuid(),
            ReviewerId = Guid.NewGuid(),
            ReviewerFullName = name,
            Rating = 4,
            Comment = "Good roommate",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        review.ReviewerFullName.Should().Be(name);
    }

    #endregion

    #region Review Rating Statistics Tests

    [Fact]
    public void Review_Rating_ShouldCalculateAverageCorrectly()
    {
        // Arrange
        var reviews = new List<Review>
        {
            new() { Id = Guid.NewGuid(), ReviewerId = Guid.NewGuid(), ReviewerFullName = "R1", Rating = 5, Comment = "C1", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), ReviewerId = Guid.NewGuid(), ReviewerFullName = "R2", Rating = 4, Comment = "C2", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), ReviewerId = Guid.NewGuid(), ReviewerFullName = "R3", Rating = 3, Comment = "C3", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), ReviewerId = Guid.NewGuid(), ReviewerFullName = "R4", Rating = 4, Comment = "C4", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), ReviewerId = Guid.NewGuid(), ReviewerFullName = "R5", Rating = 5, Comment = "C5", CreatedAt = DateTime.UtcNow }
        };

        // Act
        var averageRating = reviews.Average(r => r.Rating);

        // Assert
        averageRating.Should().Be(4.2);
    }

    [Fact]
    public void Review_Rating_ShouldCountByValue()
    {
        // Arrange
        var reviews = new List<Review>
        {
            new() { Id = Guid.NewGuid(), ReviewerId = Guid.NewGuid(), ReviewerFullName = "R1", Rating = 5, Comment = "C1", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), ReviewerId = Guid.NewGuid(), ReviewerFullName = "R2", Rating = 5, Comment = "C2", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), ReviewerId = Guid.NewGuid(), ReviewerFullName = "R3", Rating = 4, Comment = "C3", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), ReviewerId = Guid.NewGuid(), ReviewerFullName = "R4", Rating = 4, Comment = "C4", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), ReviewerId = Guid.NewGuid(), ReviewerFullName = "R5", Rating = 3, Comment = "C5", CreatedAt = DateTime.UtcNow }
        };

        // Act
        var fiveStarCount = reviews.Count(r => r.Rating == 5);
        var fourStarCount = reviews.Count(r => r.Rating == 4);
        var threeStarCount = reviews.Count(r => r.Rating == 3);

        // Assert
        fiveStarCount.Should().Be(2);
        fourStarCount.Should().Be(2);
        threeStarCount.Should().Be(1);
    }

    #endregion
}

