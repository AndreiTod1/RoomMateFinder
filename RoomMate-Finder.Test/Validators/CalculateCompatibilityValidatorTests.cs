using FluentAssertions;
using FluentValidation.TestHelper;
using RoomMate_Finder.Features.Matching.CalculateCompatibility;
using RoomMate_Finder.Validators;
using Xunit;

namespace RoomMate_Finder.Test.Validators;

public class CalculateCompatibilityValidatorTests
{
    private readonly CalculateCompatibilityValidator _validator = new();

    [Fact]
    public void Given_ValidRequest_When_Validate_Then_ShouldPass()
    {
        // Arrange
        var request = new CalculateCompatibilityRequest(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_EmptyUserId1_When_Validate_Then_ShouldFail()
    {
        // Arrange
        var request = new CalculateCompatibilityRequest(Guid.Empty, Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserId1)
            .WithErrorMessage("First user ID is required.");
    }

    [Fact]
    public void Given_EmptyUserId2_When_Validate_Then_ShouldFail()
    {
        // Arrange
        var request = new CalculateCompatibilityRequest(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserId2)
            .WithErrorMessage("Second user ID is required.");
    }

    [Fact]
    public void Given_SameUserIds_When_Validate_Then_ShouldFail()
    {
        // Arrange
        var sameId = Guid.NewGuid();
        var request = new CalculateCompatibilityRequest(sameId, sameId);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => 
            e.ErrorMessage.Contains("cannot calculate compatibility with yourself"));
    }

    [Fact]
    public void Given_BothEmptyIds_When_Validate_Then_ShouldFailWithMultipleErrors()
    {
        // Arrange
        var request = new CalculateCompatibilityRequest(Guid.Empty, Guid.Empty);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}

