using FluentValidation.TestHelper;
using CreateListingValidator = RoomMate_Finder.Features.RoomListings.CreateListing.CreateListingValidator;
using UpdateListingValidator = RoomMate_Finder.Validators.UpdateListingValidator;
using SearchListingsValidator = RoomMate_Finder.Validators.SearchListingsValidator;
using RoomMate_Finder.Features.RoomListings.CreateListing;
using RoomMate_Finder.Features.RoomListings.SearchListings;
using RoomMate_Finder.Features.RoomListings.UpdateListing;
using Xunit;

namespace RoomMate_Finder.Test.Validators;

public class ListingValidatorsTests
{
    private readonly CreateListingValidator _createValidator;
    private readonly UpdateListingValidator _updateValidator;
    private readonly SearchListingsValidator _searchValidator;

    public ListingValidatorsTests()
    {
        _createValidator = new CreateListingValidator();
        _updateValidator = new UpdateListingValidator();
        _searchValidator = new SearchListingsValidator();
    }

    // --- CreateListingValidator Tests ---

    [Fact]
    public void CreateListing_ValidRequest_Passes()
    {
        var request = new CreateListingRequest
        {
            OwnerId = Guid.NewGuid(),
            Title = "Nice Room",
            Description = "A nice room description.",
            City = "New York",
            Area = "Manhattan",
            Price = 500,
            AvailableFrom = DateTime.UtcNow.AddDays(1),
            Amenities = new List<string> { "WiFi", "AC" }
        };
        var result = _createValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateListing_InvalidPrice_Fails()
    {
        var request = new CreateListingRequest
        {
            OwnerId = Guid.NewGuid(),
            Price = 0 // Must be > 0
        };
        var result = _createValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void CreateListing_PastDate_Fails()
    {
        var request = new CreateListingRequest
        {
            OwnerId = Guid.NewGuid(),
            AvailableFrom = DateTime.UtcNow.AddDays(-1)
        };
        var result = _createValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.AvailableFrom);
    }

    [Fact]
    public void CreateListing_NullAmenities_Fails()
    {
        var request = new CreateListingRequest
        {
            OwnerId = Guid.NewGuid(),
            Title = "Test",
            Description = "Test Desc",
            City = "City",
            Area = "Area",
            Price = 100,
            AvailableFrom = DateTime.UtcNow.AddDays(1),
            Amenities = null!
        };
        var result = _createValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Amenities);
    }

    [Fact]
    public void CreateListing_EmptyAmenities_Passes()
    {
        var request = new CreateListingRequest
        {
            OwnerId = Guid.NewGuid(),
            Title = "Test",
            Description = "Test Desc",
            City = "City",
            Area = "Area",
            Price = 100,
            AvailableFrom = DateTime.UtcNow.AddDays(1),
            Amenities = new List<string>()
        };
        var result = _createValidator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Amenities);
    }

    // --- UpdateListingValidator Tests ---

    [Fact]
    public void UpdateListing_ValidRequest_Passes()
    {
        var request = new UpdateListingRequest
        {
            Id = Guid.NewGuid(),
            Title = "Updated Title",
            Description = "Updated Desc",
            City = "Paris",
            Area = "Center",
            Price = 600,
            AvailableFrom = DateTime.UtcNow.AddDays(10),
            Amenities = new List<string> { "TV" }
        };
        var result = _updateValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateListing_InvalidTitleLength_Fails()
    {
        var request = new UpdateListingRequest
        {
            Id = Guid.NewGuid(),
            Title = new string('a', 201) // Too long
        };
        var result = _updateValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    // --- SearchListingsValidator Tests ---

    [Fact]
    public void SearchListings_ValidRequest_Passes()
    {
        var request = new SearchListingsRequest
        {
            Page = 1,
            PageSize = 20,
            MinPrice = 100,
            MaxPrice = 500
        };
        var result = _searchValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SearchListings_MinPriceGreaterThanMax_Fails()
    {
        var request = new SearchListingsRequest
        {
            MinPrice = 600,
            MaxPrice = 500
        };
        var result = _searchValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.MinPrice);
    }

    [Fact]
    public void SearchListings_InvalidPage_Fails()
    {
        var request = new SearchListingsRequest
        {
            Page = 0
        };
        var result = _searchValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }
}
