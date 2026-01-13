using FluentAssertions;
using FluentValidation.TestHelper;
using RoomMate_Finder.Features.Profiles.UpdateProfile;
using RoomMate_Finder.Validators;
using Xunit;

namespace RoomMate_Finder.Test.Validators;

public class UpdateProfileValidatorTests
{
    private UpdateProfileValidator _validator;

    public UpdateProfileValidatorTests()
    {
        _validator = CreateSut();
    }

    private static UpdateProfileValidator CreateSut() => new();

    [Fact]
    public void Given_TooLongFullName_When_Validate_Then_ShouldReturnInvalidResult()
    {
        // Arrange
        var longName = new string('a', 51);
        var model = new UpdateProfileRequest(
            FullName: longName,
            Age: 25,
            Gender: "M",
            University: "Test University",
            Bio: "Test bio",
            Lifestyle: "Active",
            Interests: "Coding");

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
        result.Errors[0].ErrorMessage.Should().Be("FullName must be at most 50 characters long.");
    }

    [Fact]
    public void Given_InvalidAge_When_Validate_Then_ShouldReturnInvalidResult()
    {
        // Arrange
        var model = new UpdateProfileRequest(
            FullName: "Test User",
            Age: 10, // Invalid - too young
            Gender: "M",
            University: "Test University",
            Bio: "Test bio",
            Lifestyle: "Active",
            Interests: "Coding");

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.ShouldHaveValidationErrorFor(x => x.Age);
        result.Errors[0].ErrorMessage.Should().Be("Age must be between 16 and 100.");
    }

    [Fact]
    public void Given_EmptyGender_When_Validate_Then_ShouldReturnInvalidResult()
    {
        // Arrange
        var model = new UpdateProfileRequest(
            FullName: "Test User",
            Age: 25,
            Gender: string.Empty, // Invalid - empty
            University: "Test University",
            Bio: "Test bio",
            Lifestyle: "Active",
            Interests: "Coding");

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.ShouldHaveValidationErrorFor(x => x.Gender);
        result.Errors[0].ErrorMessage.Should().Be("Gender cannot be empty.");
    }

    [Fact]
    public void Given_EmptyUniversity_When_Validate_Then_ShouldReturnInvalidResult()
    {
        // Arrange
        var model = new UpdateProfileRequest(
            FullName: "Test User",
            Age: 25,
            Gender: "M",
            University: string.Empty, // Invalid - empty
            Bio: "Test bio",
            Lifestyle: "Active",
            Interests: "Coding");

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.ShouldHaveValidationErrorFor(x => x.University);
        result.Errors[0].ErrorMessage.Should().Be("University cannot be empty.");
    }

    [Fact]
    public void Given_TooLongBio_When_Validate_Then_ShouldReturnInvalidResult()
    {
        // Arrange
        var longBio = new string('a', 501);
        var model = new UpdateProfileRequest(
            FullName: "Test User",
            Age: 25,
            Gender: "M",
            University: "Test University",
            Bio: longBio, // Invalid - too long
            Lifestyle: "Active",
            Interests: "Coding");

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.ShouldHaveValidationErrorFor(x => x.Bio);
        result.Errors[0].ErrorMessage.Should().Be("Bio must be at most 500 characters long.");
    }

    [Fact]
    public void Given_TooLongLifestyle_When_Validate_Then_ShouldReturnInvalidResult()
    {
        // Arrange
        var longLifestyle = new string('a', 101);
        var model = new UpdateProfileRequest(
            FullName: "Test User",
            Age: 25,
            Gender: "M",
            University: "Test University",
            Bio: "Test bio",
            Lifestyle: longLifestyle, // Invalid - too long
            Interests: "Coding");

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.ShouldHaveValidationErrorFor(x => x.Lifestyle);
        result.Errors[0].ErrorMessage.Should().Be("Lifestyle must be at most 100 characters long.");
    }

    [Fact]
    public void Given_TooLongInterests_When_Validate_Then_ShouldReturnInvalidResult()
    {
        // Arrange
        var longInterests = new string('a', 201);
        var model = new UpdateProfileRequest(
            FullName: "Test User",
            Age: 25,
            Gender: "M",
            University: "Test University",
            Bio: "Test bio",
            Lifestyle: "Active",
            Interests: longInterests); // Invalid - too long

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.ShouldHaveValidationErrorFor(x => x.Interests);
        result.Errors[0].ErrorMessage.Should().Be("Interests must be at most 200 characters long.");
    }

    [Fact]
    public void Given_AllNullValues_When_Validate_Then_ShouldReturnValidResult()
    {
        // Arrange
        var model = new UpdateProfileRequest(
            FullName: null,
            Age: null,
            Gender: null,
            University: null,
            Bio: null,
            Lifestyle: null,
            Interests: null);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Given_ValidUpdateRequest_When_Validate_Then_ShouldReturnValidResult()
    {
        // Arrange
        var model = new UpdateProfileRequest(
            FullName: "Test User",
            Age: 25,
            Gender: "M",
            University: "Test University",
            Bio: "Test bio",
            Lifestyle: "Active",
            Interests: "Coding, Music");

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
