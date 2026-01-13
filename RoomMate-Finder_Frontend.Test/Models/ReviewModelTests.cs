using FluentAssertions;
using RoomMate_Finder_Frontend.Models;

namespace RoomMate_Finder_Frontend.Test.Models;

public class ReviewModelTests
{
    [Fact]
    public void Review_Should_HaveDefaultValues()
    {
        // Act
        var review = new Review
        {
            ReviewerFullName = "Test Reviewer",
            Comment = "Great roommate!"
        };

        // Assert
        review.Id.Should().Be(Guid.Empty);
        review.ReviewerId.Should().Be(Guid.Empty);
        review.Rating.Should().Be(0);
        review.CreatedAt.Should().Be(default);
    }

    [Fact]
    public void Review_Should_SetPropertiesCorrectly()
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
            ReviewerFullName = "John Doe",
            Rating = 5,
            Comment = "Excellent roommate, highly recommend!",
            CreatedAt = createdAt
        };

        // Assert
        review.Id.Should().Be(id);
        review.ReviewerId.Should().Be(reviewerId);
        review.ReviewerFullName.Should().Be("John Doe");
        review.Rating.Should().Be(5);
        review.Comment.Should().Be("Excellent roommate, highly recommend!");
        review.CreatedAt.Should().Be(createdAt);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Review_Rating_Should_AcceptValidValues(int rating)
    {
        // Act
        var review = new Review
        {
            ReviewerFullName = "Test",
            Comment = "Test comment",
            Rating = rating
        };

        // Assert
        review.Rating.Should().Be(rating);
    }

    [Fact]
    public void Review_Should_AllowEmptyComment()
    {
        // Act
        var review = new Review
        {
            ReviewerFullName = "Test",
            Comment = "",
            Rating = 3
        };

        // Assert
        review.Comment.Should().BeEmpty();
    }

    [Fact]
    public void Review_Should_AllowLongComment()
    {
        // Arrange
        var longComment = new string('a', 1000);

        // Act
        var review = new Review
        {
            ReviewerFullName = "Test",
            Comment = longComment,
            Rating = 4
        };

        // Assert
        review.Comment.Length.Should().Be(1000);
    }

    [Fact]
    public void Review_CreatedAt_Should_BeSettable()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-30);
        var futureDate = DateTime.UtcNow.AddDays(1);

        // Act
        var pastReview = new Review
        {
            ReviewerFullName = "Test",
            Comment = "Old review",
            CreatedAt = pastDate
        };
        var futureReview = new Review
        {
            ReviewerFullName = "Test",
            Comment = "Future review",
            CreatedAt = futureDate
        };

        // Assert
        pastReview.CreatedAt.Should().Be(pastDate);
        futureReview.CreatedAt.Should().Be(futureDate);
    }

    [Fact]
    public void Review_Should_HandleSpecialCharactersInComment()
    {
        // Act
        var review = new Review
        {
            ReviewerFullName = "Test User",
            Comment = "Great! 👍 Special chars: @#$%^&*()_+-=[]{}|;':\",./<>?",
            Rating = 5
        };

        // Assert
        review.Comment.Should().Contain("👍");
        review.Comment.Should().Contain("@#$%");
    }

    [Fact]
    public void Review_Should_HandleUnicodeInReviewerName()
    {
        // Act
        var review = new Review
        {
            ReviewerFullName = "José García",
            Comment = "Buen compañero de cuarto",
            Rating = 4
        };

        // Assert
        review.ReviewerFullName.Should().Be("José García");
        review.Comment.Should().Contain("compañero");
    }
}

