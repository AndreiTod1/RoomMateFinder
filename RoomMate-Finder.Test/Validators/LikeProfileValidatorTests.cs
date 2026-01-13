using FluentAssertions;
using FluentValidation.TestHelper;
using RoomMate_Finder.Features.Matching.LikeProfile;
using RoomMate_Finder.Validators;
using Xunit;

namespace RoomMate_Finder.Test.Validators;

public class LikeProfileValidatorTests
{
    private readonly LikeProfileValidator _validator = new();

    [Fact]
    public void Given_ValidRequest_When_Validate_Then_ShouldPass()
    {
        // Arrange
        var request = new LikeProfileRequest(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyUserId_When_Validate_Then_ShouldFail()
    {
        // Arrange
        var request = new LikeProfileRequest(Guid.Empty, Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("User ID is required.");
    }

    [Fact]
    public void Given_EmptyTargetUserId_When_Validate_Then_ShouldFail()
    {
        // Arrange
        var request = new LikeProfileRequest(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.TargetUserId)
            .WithErrorMessage("Target user ID is required.");
    }

    [Fact]
    public void Given_SameUserAndTargetId_When_Validate_Then_ShouldFail()
    {
        // Arrange
        var sameId = Guid.NewGuid();
        var request = new LikeProfileRequest(sameId, sameId);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.ErrorMessage.Contains("Cannot like yourself"));
    }

    [Fact]
    public void Given_BothEmptyIds_When_Validate_Then_ShouldFailWithMultipleErrors()
    {
        // Arrange
        var request = new LikeProfileRequest(Guid.Empty, Guid.Empty);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void Given_ValidDifferentIds_When_Validate_Then_ShouldPass()
    {
        // Arrange
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var targetId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var request = new LikeProfileRequest(userId, targetId);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

