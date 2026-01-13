﻿using FluentValidation;

namespace RoomMate_Finder.Features.RoomListings.CreateListing;

public class CreateListingValidator : AbstractValidator<CreateListingRequest>
{
    public CreateListingValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty()
            .WithMessage("OwnerId is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(100)
            .WithMessage("Title cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(2000)
            .WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required")
            .MaximumLength(50)
            .WithMessage("City cannot exceed 50 characters");

        RuleFor(x => x.Area)
            .NotEmpty()
            .WithMessage("Area is required")
            .MaximumLength(100)
            .WithMessage("Area cannot exceed 100 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0");

        RuleFor(x => x.AvailableFrom)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Available from date must be today or in the future");

        RuleFor(x => x.Amenities)
            .NotNull()
            .WithMessage("Amenities list cannot be null");

        When(x => x.Amenities != null, () =>
        {
            RuleFor(x => x.Amenities)
                .Must(amenities => amenities!.Count <= 20)
                .WithMessage("Cannot have more than 20 amenities")
                .Must(amenities => amenities!.All(a => !string.IsNullOrWhiteSpace(a) && a.Length <= 50))
                .WithMessage("Each amenity must be non-empty and not exceed 50 characters");
        });
    }
}
