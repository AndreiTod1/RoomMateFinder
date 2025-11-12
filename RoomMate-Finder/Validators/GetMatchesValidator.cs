using FluentValidation;
using RoomMate_Finder.Features.Matching.GetMatches;

namespace RoomMate_Finder.Validators;

public class GetMatchesValidator : AbstractValidator<GetMatchesRequest>
{
    public GetMatchesValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}
