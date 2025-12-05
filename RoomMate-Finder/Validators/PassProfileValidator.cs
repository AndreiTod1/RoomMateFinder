using FluentValidation;
using RoomMate_Finder.Features.Matching.PassProfile;

namespace RoomMate_Finder.Validators;

public class PassProfileValidator : AbstractValidator<PassProfileRequest>
{
    public PassProfileValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.TargetUserId)
            .NotEmpty()
            .WithMessage("Target user ID is required.");

        RuleFor(x => x)
            .Must(x => x.UserId != x.TargetUserId)
            .WithMessage("Cannot pass yourself - User ID and Target User ID must be different.");
    }
}
