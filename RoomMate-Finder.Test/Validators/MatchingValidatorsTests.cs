using FluentAssertions;
using FluentValidation.TestHelper;
using RoomMate_Finder.Features.Matching.CalculateCompatibility;
using RoomMate_Finder.Features.Matching.GetMatches;
using RoomMate_Finder.Features.Matching.GetUserMatches;
using RoomMate_Finder.Features.Matching.LikeProfile;
using RoomMate_Finder.Features.Matching.PassProfile;
using RoomMate_Finder.Validators;
using Xunit;

namespace RoomMate_Finder.Test.Validators;

public class MatchingValidatorsTests
{
    #region CalculateCompatibilityValidator Tests

    private readonly CalculateCompatibilityValidator _compatibilityValidator = new();

    [Fact]
    public void CalculateCompatibility_Given_EmptyUserId1_When_Validate_Then_ShouldFail()
    {
        var request = new CalculateCompatibilityRequest(Guid.Empty, Guid.NewGuid());

        var result = _compatibilityValidator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserId1)
            .WithErrorMessage("First user ID is required.");
    }

    [Fact]
    public void CalculateCompatibility_Given_EmptyUserId2_When_Validate_Then_ShouldFail()
    {
        var request = new CalculateCompatibilityRequest(Guid.NewGuid(), Guid.Empty);

        var result = _compatibilityValidator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserId2)
            .WithErrorMessage("Second user ID is required.");
    }

    [Fact]
    public void CalculateCompatibility_Given_SameUserIds_When_Validate_Then_ShouldFail()
    {
        var userId = Guid.NewGuid();
        var request = new CalculateCompatibilityRequest(userId, userId);

        var result = _compatibilityValidator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("cannot calculate compatibility with yourself"));
    }

    [Fact]
    public void CalculateCompatibility_Given_ValidRequest_When_Validate_Then_ShouldPass()
    {
        var request = new CalculateCompatibilityRequest(Guid.NewGuid(), Guid.NewGuid());

        var result = _compatibilityValidator.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region GetMatchesValidator Tests

    private readonly GetMatchesValidator _getMatchesValidator = new();

    [Fact]
    public void GetMatches_Given_EmptyUserId_When_Validate_Then_ShouldFail()
    {
        var request = new GetMatchesRequest(Guid.Empty);

        var result = _getMatchesValidator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("User ID is required.");
    }

    [Fact]
    public void GetMatches_Given_ValidUserId_When_Validate_Then_ShouldPass()
    {
        var request = new GetMatchesRequest(Guid.NewGuid());

        var result = _getMatchesValidator.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region GetUserMatchesValidator Tests

    private readonly GetUserMatchesValidator _getUserMatchesValidator = new();

    [Fact]
    public void GetUserMatches_Given_EmptyUserId_When_Validate_Then_ShouldFail()
    {
        var request = new GetUserMatchesRequest(Guid.Empty);

        var result = _getUserMatchesValidator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("User ID is required.");
    }

    [Fact]
    public void GetUserMatches_Given_ValidUserId_When_Validate_Then_ShouldPass()
    {
        var request = new GetUserMatchesRequest(Guid.NewGuid());

        var result = _getUserMatchesValidator.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region LikeProfileValidator Tests

    private readonly LikeProfileValidator _likeProfileValidator = new();

    [Fact]
    public void LikeProfile_Given_EmptyUserId_When_Validate_Then_ShouldFail()
    {
        var request = new LikeProfileRequest(Guid.Empty, Guid.NewGuid());

        var result = _likeProfileValidator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("User ID is required.");
    }

    [Fact]
    public void LikeProfile_Given_EmptyTargetUserId_When_Validate_Then_ShouldFail()
    {
        var request = new LikeProfileRequest(Guid.NewGuid(), Guid.Empty);

        var result = _likeProfileValidator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.TargetUserId)
            .WithErrorMessage("Target user ID is required.");
    }

    [Fact]
    public void LikeProfile_Given_SameUserIds_When_Validate_Then_ShouldFail()
    {
        var userId = Guid.NewGuid();
        var request = new LikeProfileRequest(userId, userId);

        var result = _likeProfileValidator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Cannot like yourself"));
    }

    [Fact]
    public void LikeProfile_Given_ValidRequest_When_Validate_Then_ShouldPass()
    {
        var request = new LikeProfileRequest(Guid.NewGuid(), Guid.NewGuid());

        var result = _likeProfileValidator.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region PassProfileValidator Tests

    private readonly PassProfileValidator _passProfileValidator = new();

    [Fact]
    public void PassProfile_Given_EmptyUserId_When_Validate_Then_ShouldFail()
    {
        var request = new PassProfileRequest(Guid.Empty, Guid.NewGuid());

        var result = _passProfileValidator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("User ID is required.");
    }

    [Fact]
    public void PassProfile_Given_EmptyTargetUserId_When_Validate_Then_ShouldFail()
    {
        var request = new PassProfileRequest(Guid.NewGuid(), Guid.Empty);

        var result = _passProfileValidator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.TargetUserId)
            .WithErrorMessage("Target user ID is required.");
    }

    [Fact]
    public void PassProfile_Given_SameUserIds_When_Validate_Then_ShouldFail()
    {
        var userId = Guid.NewGuid();
        var request = new PassProfileRequest(userId, userId);

        var result = _passProfileValidator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Cannot pass yourself"));
    }

    [Fact]
    public void PassProfile_Given_ValidRequest_When_Validate_Then_ShouldPass()
    {
        var request = new PassProfileRequest(Guid.NewGuid(), Guid.NewGuid());

        var result = _passProfileValidator.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
