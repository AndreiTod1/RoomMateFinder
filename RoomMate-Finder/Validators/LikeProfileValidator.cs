using FluentValidation;
using RoomMate_Finder.Features.Matching.LikeProfile;

namespace RoomMate_Finder.Validators;

public class LikeProfileValidator : AbstractValidator<LikeProfileRequest>
{
    public LikeProfileValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.TargetUserId)
            .NotEmpty()
            .WithMessage("Target user ID is required.");

        RuleFor(x => x)
            .Must(x => x.UserId != x.TargetUserId)
            .WithMessage("Cannot like yourself - User ID and Target User ID must be different.");
    }
}
