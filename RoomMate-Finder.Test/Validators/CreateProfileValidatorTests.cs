using FluentAssertions;
using FluentValidation.TestHelper;
using RoomMate_Finder.Features.Profiles;
using RoomMate_Finder.Validators;
using Xunit;

namespace RoomMate_Finder.Test.Validators;

public class CreateProfileValidatorTests
{
    private CreateProfileValidator _validator;

    public CreateProfileValidatorTests()
    {
        _validator = CreateSut();
    }

    private static CreateProfileValidator CreateSut() => new();

    [Fact]
    public void Given_EmptyFullName_When_Validate_Then_ShouldReturnInvalidResult()
    {
        // Arrange
        var model = new CreateProfileRequest(
            Email: "test@example.com",
            Password: "Str0ng!Pass1!",
            FullName: string.Empty,
            Bio: "Some bio",
            Age: 25,
            Gender: "M",
            University: "Uni",
            Lifestyle: "Calm",
            Interests: "Coding");

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Given_TooLongFullName_When_Validate_Then_ShouldReturnInvalidResult()
    {
        // Arrange
        var longName = new string('a', 51);
        var model = new CreateProfileRequest(
            Email: "test@example.com",
            Password: "Str0ng!Pass1!",
            FullName: longName,
            Bio: "Some bio",
            Age: 25,
            Gender: "M",
            University: "Uni",
            Lifestyle: "Calm",
            Interests: "Coding");

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Given_InvalidEmail_When_Validate_Then_ShouldReturnInvalidResult()
    {
        // Arrange
        var model = new CreateProfileRequest(
            Email: "invalid-email",
            Password: "Str0ng!Pass1!",
            FullName: "Valid Name",
            Bio: "Some bio",
            Age: 25,
            Gender: "M",
            University: "Uni",
            Lifestyle: "Calm",
            Interests: "Coding");

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Given_ValidRequest_When_Validate_Then_ShouldReturnValidResult()
    {
        // Arrange
        var model = new CreateProfileRequest(
            Email: "test@example.com",
            Password: "Str0ng!Pass1!",
            FullName: "Valid Name",
            Bio: "Some bio",
            Age: 25,
            Gender: "M",
            University: "Uni",
            Lifestyle: "Calm",
            Interests: "Coding");

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
