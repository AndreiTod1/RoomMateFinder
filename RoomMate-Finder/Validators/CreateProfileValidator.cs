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
    }
}