using FluentAssertions;
using FluentValidation.TestHelper;
using RoomMate_Finder.Features.Conversations.StartConversation;
using RoomMate_Finder.Validators;
using Xunit;

namespace RoomMate_Finder.Test.Validators;

public class StartConversationValidatorTests
{
    private StartConversationValidator _validator;

    public StartConversationValidatorTests()
    {
        _validator = CreateSut();
    }

    private static StartConversationValidator CreateSut() => new();

    [Fact]
    public void Given_EmptyUserId_When_Validate_Then_ShouldReturnInvalidResult()
    {
        // Arrange
        var model = new StartConversationRequest(Guid.Empty);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.ShouldHaveValidationErrorFor(x => x.OtherUserId);
        result.Errors[0].ErrorMessage.Should().Be("User ID is required");
    }

    [Fact]
    public void Given_ValidUserId_When_Validate_Then_ShouldReturnValidResult()
    {
        // Arrange
        var model = new StartConversationRequest(Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
