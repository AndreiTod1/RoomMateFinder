using FluentAssertions;
using FluentValidation.TestHelper;
using RoomMate_Finder.Features.Matching.PassProfile;
using RoomMate_Finder.Validators;
using Xunit;

namespace RoomMate_Finder.Test.Validators;

public class PassProfileValidatorTests
{
    private readonly PassProfileValidator _validator = new();

    [Fact]
    public void Given_ValidRequest_When_Validate_Then_ShouldPass()
    {
        // Arrange
        var request = new PassProfileRequest(Guid.NewGuid(), Guid.NewGuid());

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
        var request = new PassProfileRequest(Guid.Empty, Guid.NewGuid());

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
        var request = new PassProfileRequest(Guid.NewGuid(), Guid.Empty);

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
        var request = new PassProfileRequest(sameId, sameId);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.ErrorMessage.Contains("Cannot pass yourself"));
    }

    [Fact]
    public void Given_BothEmptyIds_When_Validate_Then_ShouldFailWithMultipleErrors()
    {
        // Arrange
        var request = new PassProfileRequest(Guid.Empty, Guid.Empty);

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
        var userId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var targetId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var request = new PassProfileRequest(userId, targetId);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

