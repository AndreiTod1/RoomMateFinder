using FluentAssertions;
using FluentValidation.TestHelper;
using RoomMate_Finder.Features.Matching.GetMatches;
using RoomMate_Finder.Validators;
using Xunit;

namespace RoomMate_Finder.Test.Validators;

public class GetMatchesValidatorTests
{
    private readonly GetMatchesValidator _validator = new();

    [Fact]
    public void Given_ValidUserId_When_Validate_Then_ShouldPass()
    {
        // Arrange
        var request = new GetMatchesRequest(Guid.NewGuid());

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
        var request = new GetMatchesRequest(Guid.Empty);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("User ID is required.");
    }

    [Fact]
    public void Given_ValidNewGuid_When_Validate_Then_ShouldPass()
    {
        // Arrange
        var request = new GetMatchesRequest(Guid.Parse("12345678-1234-1234-1234-123456789012"));

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

