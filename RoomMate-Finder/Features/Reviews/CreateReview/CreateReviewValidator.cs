using FluentValidation;

namespace RoomMate_Finder.Features.Reviews.CreateReview;

public class CreateReviewValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewValidator()
    {
        RuleFor(x => x.ReviewerId)
            .NotEmpty()
            .WithMessage("ReviewerId is required");

        RuleFor(x => x.ReviewedUserId)
            .NotEmpty()
            .WithMessage("ReviewedUserId is required")
            .NotEqual(x => x.ReviewerId)
            .WithMessage("Cannot review yourself");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.Comment)
            .MaximumLength(1000)
            .WithMessage("Comment cannot exceed 1000 characters");
    }
}
