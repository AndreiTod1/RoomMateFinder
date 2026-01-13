using FluentValidation.TestHelper;
using RoomMate_Finder.Features.Matching.LikeProfile;
using RoomMate_Finder.Features.Matching.PassProfile;
using RoomMate_Finder.Features.Profiles;
using RoomMate_Finder.Features.Profiles.UpdateProfile;
using RoomMate_Finder.Validators;
using Xunit;

namespace RoomMate_Finder.Test.Validators;

public class ProfileValidatorsTests
{
    private readonly CreateProfileValidator _createValidator;
    private readonly UpdateProfileValidator _updateValidator;
    private readonly LikeProfileValidator _likeValidator;
    private readonly PassProfileValidator _passValidator;

    public ProfileValidatorsTests()
    {
        _createValidator = new CreateProfileValidator();
        _updateValidator = new UpdateProfileValidator();
        _likeValidator = new LikeProfileValidator();
        _passValidator = new PassProfileValidator();
    }

    // --- CreateProfileValidator Tests ---

    [Fact]
    public void CreateProfile_ValidRequest_Passes()
    {
        var request = new CreateProfileRequest(
            Email: "test@test.com",
            Password: "Password123!",
            FullName: "John Doe",
            Bio: "Test Bio",
            Age: 25,
            Gender: "Male",
            University: "Test Uni",
            Lifestyle: "Active",
            Interests: "Coding"
        );
        var result = _createValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateProfile_InvalidFullName_Fails()
    {
        var request = new CreateProfileRequest(
            Email: "test@test.com",
            Password: "Password123!",
            FullName: "",
            Bio: "Test Bio",
            Age: 25,
            Gender: "Male",
            University: "Test Uni",
            Lifestyle: "Active",
            Interests: "Coding"
        );
        var result = _createValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void CreateProfile_InvalidAge_Fails()
    {
        var request = new CreateProfileRequest(
            Email: "test@test.com",
            Password: "Password123!",
            FullName: "John",
            Bio: "Test Bio",
            Age: 15,
            Gender: "Male",
            University: "Test Uni",
            Lifestyle: "Active",
            Interests: "Coding"
        );
        var result = _createValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Age);
    }

    [Fact]
    public void CreateProfile_InvalidPassword_Fails()
    {
        var request = new CreateProfileRequest(
            Email: "test@test.com",
            Password: "weak",
            FullName: "John",
            Bio: "Test Bio",
            Age: 25,
            Gender: "Male",
            University: "Test Uni",
            Lifestyle: "Active",
            Interests: "Coding"
        );
        var result = _createValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    // --- UpdateProfileValidator Tests ---

    [Fact]
    public void UpdateProfile_ValidSemiRequest_Passes()
    {
        var request = new UpdateProfileRequest(
            FullName: "New Name",
            Age: null,
            Gender: null,
            University: null,
            Bio: null,
            Lifestyle: null,
            Interests: null
        );
        var result = _updateValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateProfile_InvalidAgeRequest_Fails()
    {
        var request = new UpdateProfileRequest(
            FullName: null,
            Age: 101,
            Gender: null,
            University: null,
            Bio: null,
            Lifestyle: null,
            Interests: null
        );
        var result = _updateValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Age);
    }

    // --- LikeProfileValidator Tests ---

    [Fact]
    public void LikeProfile_ValidRequest_Passes()
    {
        var request = new LikeProfileRequest(Guid.NewGuid(), Guid.NewGuid());
        var result = _likeValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void LikeProfile_SameIds_Fails()
    {
        var sameId = Guid.NewGuid();
        var request = new LikeProfileRequest(sameId, sameId);
        var result = _likeValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x);
    }

    // --- PassProfileValidator Tests ---

    [Fact]
    public void PassProfile_ValidRequest_Passes()
    {
        var request = new PassProfileRequest(Guid.NewGuid(), Guid.NewGuid());
        var result = _passValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PassProfile_SameIds_Fails()
    {
        var sameId = Guid.NewGuid();
        var request = new PassProfileRequest(sameId, sameId);
        var result = _passValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x);
    }
}
