using FluentValidation.TestHelper;
using RoomMate_Finder.Features.Matching.CalculateCompatibility;
using RoomMate_Finder.Features.Matching.GetMatches;
using RoomMate_Finder.Features.Matching.GetUserMatches;
using RoomMate_Finder.Validators;
using Xunit;

namespace RoomMate_Finder.Test.Validators;

public class MatchingValidatorsTests
{
    private readonly GetMatchesValidator _getMatchesValidator;
    private readonly GetUserMatchesValidator _getUserMatchesValidator;
    private readonly CalculateCompatibilityValidator _compatibilityValidator;

    public MatchingValidatorsTests()
    {
        _getMatchesValidator = new GetMatchesValidator();
        _getUserMatchesValidator = new GetUserMatchesValidator();
        _compatibilityValidator = new CalculateCompatibilityValidator();
    }

    [Fact]
    public void GetMatches_Valid_Passes()
    {
        var request = new GetMatchesRequest(Guid.NewGuid());
        var result = _getMatchesValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void GetMatches_EmptyId_Fails()
    {
        var request = new GetMatchesRequest(Guid.Empty);
        var result = _getMatchesValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void GetUserMatches_Valid_Passes()
    {
        var request = new GetUserMatchesRequest(Guid.NewGuid());
        var result = _getUserMatchesValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void GetUserMatches_EmptyId_Fails()
    {
        var request = new GetUserMatchesRequest(Guid.Empty);
        var result = _getUserMatchesValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void CalculateCompatibility_Valid_Passes()
    {
        var request = new CalculateCompatibilityRequest(Guid.NewGuid(), Guid.NewGuid());
        var result = _compatibilityValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CalculateCompatibility_SameIds_Fails()
    {
        var sameId = Guid.NewGuid();
        var request = new CalculateCompatibilityRequest(sameId, sameId);
        var result = _compatibilityValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x);
    }
}
