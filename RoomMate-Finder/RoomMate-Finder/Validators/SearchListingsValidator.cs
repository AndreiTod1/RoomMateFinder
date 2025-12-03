using FluentValidation;
using RoomMate_Finder.Features.RoomListings.SearchListings;

namespace RoomMate_Finder.Validators;

public class SearchListingsValidator : AbstractValidator<SearchListingsRequest>
{
    public SearchListingsValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(x => x.MaxPrice ?? decimal.MaxValue)
            .When(x => x.MinPrice.HasValue);
    }
}

