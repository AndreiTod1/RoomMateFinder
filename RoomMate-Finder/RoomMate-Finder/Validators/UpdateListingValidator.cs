using FluentValidation;
using RoomMate_Finder.Features.RoomListings.UpdateListing;

namespace RoomMate_Finder.Validators;

public class UpdateListingValidator : AbstractValidator<UpdateListingRequest>
{
    public UpdateListingValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .NotEmpty();

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Area)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Price)
            .GreaterThan(0).LessThan(10000);

        RuleFor(x => x.AvailableFrom)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date);

        RuleForEach(x => x.Amenities)
            .NotEmpty()
            .MaximumLength(50);
    }
}

