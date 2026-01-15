using FluentValidation.TestHelper;
using RoomMate_Finder.Features.Conversations.StartConversation;
using RoomMate_Finder.Features.Reviews.CreateReview;
using RoomMate_Finder.Validators;
using Xunit;

namespace RoomMate_Finder.Test.Validators;

public class OtherValidatorsTests
{
    private readonly CreateReviewValidator _reviewValidator;
    private readonly StartConversationValidator _conversationValidator;

    public OtherValidatorsTests()
    {
        _reviewValidator = new CreateReviewValidator();
        _conversationValidator = new StartConversationValidator();
    }

    // --- CreateReviewValidator Tests ---

    [Fact]
    public void CreateReview_Valid_Passes()
    {
        var request = new CreateReviewRequest
        {
            ReviewerId = Guid.NewGuid(),
            ReviewedUserId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Good roommate!"
        };
        var result = _reviewValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateReview_SelfReview_Fails()
    {
        var sameId = Guid.NewGuid();
        var request = new CreateReviewRequest
        {
            ReviewerId = sameId,
            ReviewedUserId = sameId,
            Rating = 5
        };
        var result = _reviewValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ReviewedUserId);
    }

    [Fact]
    public void CreateReview_InvalidRating_Fails()
    {
        var request = new CreateReviewRequest
        {
            ReviewerId = Guid.NewGuid(),
            ReviewedUserId = Guid.NewGuid(),
            Rating = 6 // Max 5
        };
        var result = _reviewValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    // --- StartConversationValidator Tests ---

    [Fact]
    public void StartConversation_Valid_Passes()
    {
        var request = new StartConversationRequest(Guid.NewGuid());
        var result = _conversationValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void StartConversation_EmptyId_Fails()
    {
        var request = new StartConversationRequest(Guid.Empty);
        var result = _conversationValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.OtherUserId);
    }
}
