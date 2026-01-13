using FluentAssertions;
using FluentValidation.TestHelper;
using RoomMate_Finder.Features.Matching.GetUserMatches;
using RoomMate_Finder.Validators;
using Xunit;

namespace RoomMate_Finder.Test.Validators;

public class GetUserMatchesValidatorTests
{
    private readonly GetUserMatchesValidator _validator = new();

    [Fact]
    public void Given_ValidUserId_When_Validate_Then_ShouldPass()
    {
        // Arrange
        var request = new GetUserMatchesRequest(Guid.NewGuid());

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
        var request = new GetUserMatchesRequest(Guid.Empty);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("User ID is required.");
    }

    [Fact]
    public void Given_ValidSpecificGuid_When_Validate_Then_ShouldPass()
    {
        // Arrange
        var specificGuid = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        var request = new GetUserMatchesRequest(specificGuid);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}

