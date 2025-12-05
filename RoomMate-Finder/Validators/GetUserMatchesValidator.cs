using FluentValidation;
using RoomMate_Finder.Features.Matching.GetUserMatches;

namespace RoomMate_Finder.Validators;

public class GetUserMatchesValidator : AbstractValidator<GetUserMatchesRequest>
{
    public GetUserMatchesValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}
