using FluentAssertions;
using FluentValidation.TestHelper;
using RoomMate_Finder.Features.Reviews.CreateReview;

namespace RoomMate_Finder.Test.Validators;

public class CreateReviewValidatorTests
{
    private readonly CreateReviewValidator _validator = new();

    [Fact]
    public void Given_ValidRequest_When_Validate_Then_ShouldPass()
    {
        // Arrange
        var request = new CreateReviewRequest
        {
            ReviewerId = Guid.NewGuid(),
            ReviewedUserId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Great roommate!"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyReviewerId_When_Validate_Then_ShouldFail()
    {
        // Arrange
        var request = new CreateReviewRequest
        {
            ReviewerId = Guid.Empty,
            ReviewedUserId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Great!"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.ReviewerId)
            .WithErrorMessage("ReviewerId is required");
    }

    [Fact]
    public void Given_EmptyReviewedUserId_When_Validate_Then_ShouldFail()
    {
        // Arrange
        var request = new CreateReviewRequest
        {
            ReviewerId = Guid.NewGuid(),
            ReviewedUserId = Guid.Empty,
            Rating = 5,
            Comment = "Great!"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.ReviewedUserId)
            .WithErrorMessage("ReviewedUserId is required");
    }

    [Fact]
    public void Given_SameReviewerAndReviewedUser_When_Validate_Then_ShouldFail()
    {
        // Arrange
        var sameId = Guid.NewGuid();
        var request = new CreateReviewRequest
        {
            ReviewerId = sameId,
            ReviewedUserId = sameId,
            Rating = 5,
            Comment = "Great!"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.ReviewedUserId)
            .WithErrorMessage("Cannot review yourself");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(100)]
    public void Given_InvalidRating_When_Validate_Then_ShouldFail(int rating)
    {
        // Arrange
        var request = new CreateReviewRequest
        {
            ReviewerId = Guid.NewGuid(),
            ReviewedUserId = Guid.NewGuid(),
            Rating = rating,
            Comment = "Test"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Rating)
            .WithErrorMessage("Rating must be between 1 and 5");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Given_ValidRating_When_Validate_Then_ShouldPass(int rating)
    {
        // Arrange
        var request = new CreateReviewRequest
        {
            ReviewerId = Guid.NewGuid(),
            ReviewedUserId = Guid.NewGuid(),
            Rating = rating,
            Comment = "Test"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void Given_CommentTooLong_When_Validate_Then_ShouldFail()
    {
        // Arrange
        var longComment = new string('a', 1001);
        var request = new CreateReviewRequest
        {
            ReviewerId = Guid.NewGuid(),
            ReviewedUserId = Guid.NewGuid(),
            Rating = 5,
            Comment = longComment
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Comment)
            .WithErrorMessage("Comment cannot exceed 1000 characters");
    }

    [Fact]
    public void Given_CommentExactly1000Chars_When_Validate_Then_ShouldPass()
    {
        // Arrange
        var exactComment = new string('a', 1000);
        var request = new CreateReviewRequest
        {
            ReviewerId = Guid.NewGuid(),
            ReviewedUserId = Guid.NewGuid(),
            Rating = 5,
            Comment = exactComment
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Comment);
    }

    [Fact]
    public void Given_EmptyComment_When_Validate_Then_ShouldPass()
    {
        // Arrange
        var request = new CreateReviewRequest
        {
            ReviewerId = Guid.NewGuid(),
            ReviewedUserId = Guid.NewGuid(),
            Rating = 5,
            Comment = ""
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Comment);
    }

    [Fact]
    public void Given_NullComment_When_Validate_Then_ShouldPass()
    {
        // Arrange
        var request = new CreateReviewRequest
        {
            ReviewerId = Guid.NewGuid(),
            ReviewedUserId = Guid.NewGuid(),
            Rating = 5,
            Comment = null!
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Comment);
    }
}

