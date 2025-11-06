using FluentValidation;
using RoomMate_Finder.Features.Profiles;

namespace RoomMate_Finder.Validators;

public class CreateProfileValidator : AbstractValidator<CreateProfileRequest>
{
    public CreateProfileValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("FullName must be at most 50 characters long.");

        RuleFor(x => x.Age).InclusiveBetween(16, 100);
        RuleFor(x => x.Gender).NotEmpty();
        RuleFor(x => x.University).NotEmpty();
        RuleFor(x => x.Bio).MaximumLength(500);
        RuleFor(x => x.Lifestyle).MaximumLength(100);
        RuleFor(x => x.Interests).MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
}