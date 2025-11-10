using FluentValidation;
using RoomMate_Finder.Features.Profiles.UpdateProfile;

namespace RoomMate_Finder.Validators;

public class UpdateProfileValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.FullName)
            .MaximumLength(50)
            .WithMessage("FullName must be at most 50 characters long.")
            .When(x => x.FullName != null);

        RuleFor(x => x.Age)
            .InclusiveBetween(16, 100)
            .WithMessage("Age must be between 16 and 100.")
            .When(x => x.Age.HasValue);

        RuleFor(x => x.Gender)
            .NotEmpty()
            .WithMessage("Gender cannot be empty.")
            .When(x => x.Gender != null);

        RuleFor(x => x.University)
            .NotEmpty()
            .WithMessage("University cannot be empty.")
            .When(x => x.University != null);

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .WithMessage("Bio must be at most 500 characters long.")
            .When(x => x.Bio != null);

        RuleFor(x => x.Lifestyle)
            .MaximumLength(100)
            .WithMessage("Lifestyle must be at most 100 characters long.")
            .When(x => x.Lifestyle != null);

        RuleFor(x => x.Interests)
            .MaximumLength(200)
            .WithMessage("Interests must be at most 200 characters long.")
            .When(x => x.Interests != null);
    }
}
