using FluentValidation.TestHelper;
using RoomMate_Finder.Features.Profiles.Login;
using RoomMate_Finder.Validators;
using Xunit;

namespace RoomMate_Finder.Test.Validators;

public class AuthValidatorsTests
{
    private readonly LoginValidator _loginValidator;

    public AuthValidatorsTests()
    {
        _loginValidator = new LoginValidator();
    }

    [Fact]
    public void Login_ValidRequest_Passes()
    {
        var request = new LoginRequest("test@test.com", "Password123!");
        var result = _loginValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Login_InvalidEmail_Fails()
    {
        var request = new LoginRequest("invalid-email", "Password123!");
        var result = _loginValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Login_EmptyPassword_Fails()
    {
        var request = new LoginRequest("test@test.com", "");
        var result = _loginValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
