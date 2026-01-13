using FluentAssertions;
using FluentValidation.TestHelper;
using RoomMate_Finder.Features.RoomListings.CreateListing;
using CreateListingValidator = RoomMate_Finder.Validators.CreateListingValidator;
using Xunit;

namespace RoomMate_Finder.Test.Validators;

public class CreateListingValidatorTests
{
    private readonly CreateListingValidator _validator = new();

    private static CreateListingRequest CreateValidRequest() => new()
    {
        OwnerId = Guid.NewGuid(),
        Title = "Cozy Room in City Center",
        Description = "A nice room with great views and all amenities",
        City = "Bucharest",
        Area = "Sector 1",
        Price = 500,
        AvailableFrom = DateTime.UtcNow.Date.AddDays(1),
        Amenities = new List<string> { "WiFi", "Parking" }
    };

    #region Title Validation

    [Fact]
    public void Given_EmptyTitle_When_Validate_Then_ShouldFail()
    {
        var request = CreateValidRequest();
        request.Title = string.Empty;

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Given_TooLongTitle_When_Validate_Then_ShouldFail()
    {
        var request = CreateValidRequest();
        request.Title = new string('a', 151);

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Given_ValidTitle_When_Validate_Then_ShouldPass()
    {
        var request = CreateValidRequest();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Given_MaxLengthTitle_When_Validate_Then_ShouldPass()
    {
        var request = CreateValidRequest();
        request.Title = new string('a', 150);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    #endregion

    #region Description Validation

    [Fact]
    public void Given_EmptyDescription_When_Validate_Then_ShouldFail()
    {
        var request = CreateValidRequest();
        request.Description = string.Empty;

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Given_ValidDescription_When_Validate_Then_ShouldPass()
    {
        var request = CreateValidRequest();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    #endregion

    #region City Validation

    [Fact]
    public void Given_EmptyCity_When_Validate_Then_ShouldFail()
    {
        var request = CreateValidRequest();
        request.City = string.Empty;

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public void Given_TooLongCity_When_Validate_Then_ShouldFail()
    {
        var request = CreateValidRequest();
        request.City = new string('c', 101);

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public void Given_ValidCity_When_Validate_Then_ShouldPass()
    {
        var request = CreateValidRequest();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.City);
    }

    #endregion

    #region Area Validation

    [Fact]
    public void Given_EmptyArea_When_Validate_Then_ShouldFail()
    {
        var request = CreateValidRequest();
        request.Area = string.Empty;

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Area);
    }

    [Fact]
    public void Given_TooLongArea_When_Validate_Then_ShouldFail()
    {
        var request = CreateValidRequest();
        request.Area = new string('a', 101);

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Area);
    }

    [Fact]
    public void Given_ValidArea_When_Validate_Then_ShouldPass()
    {
        var request = CreateValidRequest();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Area);
    }

    #endregion

    #region Price Validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Given_ZeroOrNegativePrice_When_Validate_Then_ShouldFail(decimal price)
    {
        var request = CreateValidRequest();
        request.Price = price;

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Given_PriceTooHigh_When_Validate_Then_ShouldFail()
    {
        var request = CreateValidRequest();
        request.Price = 10001;

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Price);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(500)]
    [InlineData(9999)]
    public void Given_ValidPrice_When_Validate_Then_ShouldPass(decimal price)
    {
        var request = CreateValidRequest();
        request.Price = price;

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    #endregion

    #region AvailableFrom Validation

    [Fact]
    public void Given_PastDate_When_Validate_Then_ShouldFail()
    {
        var request = CreateValidRequest();
        request.AvailableFrom = DateTime.UtcNow.Date.AddDays(-1);

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.AvailableFrom);
    }

    [Fact]
    public void Given_TodayDate_When_Validate_Then_ShouldPass()
    {
        var request = CreateValidRequest();
        request.AvailableFrom = DateTime.UtcNow.Date;

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.AvailableFrom);
    }

    [Fact]
    public void Given_FutureDate_When_Validate_Then_ShouldPass()
    {
        var request = CreateValidRequest();
        request.AvailableFrom = DateTime.UtcNow.Date.AddMonths(1);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.AvailableFrom);
    }

    #endregion

    #region Amenities Validation

    [Fact]
    public void Given_EmptyAmenityInList_When_Validate_Then_ShouldFail()
    {
        var request = CreateValidRequest();
        request.Amenities = new List<string> { "WiFi", "", "Parking" };

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor("Amenities[1]");
    }

    [Fact]
    public void Given_TooLongAmenity_When_Validate_Then_ShouldFail()
    {
        var request = CreateValidRequest();
        request.Amenities = new List<string> { new string('a', 51) };

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor("Amenities[0]");
    }

    [Fact]
    public void Given_ValidAmenities_When_Validate_Then_ShouldPass()
    {
        var request = CreateValidRequest();
        request.Amenities = new List<string> { "WiFi", "Parking", "Air Conditioning" };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Amenities);
    }

    [Fact]
    public void Given_EmptyAmenitiesList_When_Validate_Then_ShouldPass()
    {
        var request = CreateValidRequest();
        request.Amenities = new List<string>();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Amenities);
    }

    #endregion

    #region Full Validation

    [Fact]
    public void Given_ValidRequest_When_Validate_Then_ShouldPass()
    {
        var request = CreateValidRequest();

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion
}
