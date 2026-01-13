using FluentAssertions;
using FluentValidation.TestHelper;
using RoomMate_Finder.Features.Profiles.Login;
using RoomMate_Finder.Validators;
using Xunit;

namespace RoomMate_Finder.Test.Validators;

public class LoginValidatorTests : IDisposable
{
    private LoginValidator _validator;
    private bool _disposed;

    public LoginValidatorTests()
    {
        _validator = CreateSut();
    }

    private static LoginValidator CreateSut() => new();

    [Fact]
    public void Given_EmptyEmail_When_Validate_Then_ShouldReturnInvalidResult()
    {
        // Arrange
        var model = new LoginRequest(
            Email: string.Empty,
            Password: "ValidPassword123!");

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2); // NotEmpty și EmailAddress
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Given_InvalidEmail_When_Validate_Then_ShouldReturnInvalidResult()
    {
        // Arrange
        var model = new LoginRequest(
            Email: "invalid-email",
            Password: "ValidPassword123!");

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.Errors[0].ErrorMessage.Should().Be("A valid email address is required.");
    }

    [Fact]
    public void Given_EmptyPassword_When_Validate_Then_ShouldReturnInvalidResult()
    {
        // Arrange
        var model = new LoginRequest(
            Email: "test@example.com",
            Password: string.Empty);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.ShouldHaveValidationErrorFor(x => x.Password);
        result.Errors[0].ErrorMessage.Should().Be("Password is required.");
    }

    [Fact]
    public void Given_ValidLoginRequest_When_Validate_Then_ShouldReturnValidResult()
    {
        // Arrange
        var model = new LoginRequest(
            Email: "test@example.com",
            Password: "ValidPassword123!");

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                _validator = null!;
            }

            // Dispose unmanaged resources (if any)
            
            _disposed = true;
        }
    }
}
