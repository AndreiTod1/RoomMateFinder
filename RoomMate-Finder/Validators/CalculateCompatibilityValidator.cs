using FluentValidation;
using RoomMate_Finder.Features.Matching.CalculateCompatibility;

namespace RoomMate_Finder.Validators;

public class CalculateCompatibilityValidator : AbstractValidator<CalculateCompatibilityRequest>
{
    public CalculateCompatibilityValidator()
    {
        RuleFor(x => x.UserId1)
            .NotEmpty()
            .WithMessage("First user ID is required.");

        RuleFor(x => x.UserId2)
            .NotEmpty()
            .WithMessage("Second user ID is required.");

        RuleFor(x => x)
            .Must(x => x.UserId1 != x.UserId2)
            .WithMessage("User IDs must be different - cannot calculate compatibility with yourself.");
    }
}
