using FluentAssertions;
using FluentValidation.TestHelper;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.RoomListings.SearchListings;
using RoomMate_Finder.Validators;
using Xunit;

namespace RoomMate_Finder.Test.Validators;

public class SearchListingsValidatorTests
{
    private readonly SearchListingsValidator _validator = new();

    private static SearchListingsRequest CreateValidRequest() => new()
    {
        Page = 1,
        PageSize = 20,
        City = null,
        Area = null,
        MinPrice = null,
        MaxPrice = null
    };

    #region Page Validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Given_InvalidPage_When_Validate_Then_ShouldFail(int page)
    {
        var request = CreateValidRequest();
        request.Page = page;

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public void Given_ValidPage_When_Validate_Then_ShouldPass(int page)
    {
        var request = CreateValidRequest();
        request.Page = page;

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Page);
    }

    #endregion

    #region PageSize Validation

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(200)]
    public void Given_InvalidPageSize_When_Validate_Then_ShouldFail(int pageSize)
    {
        var request = CreateValidRequest();
        request.PageSize = pageSize;

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Given_ValidPageSize_When_Validate_Then_ShouldPass(int pageSize)
    {
        var request = CreateValidRequest();
        request.PageSize = pageSize;

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }

    #endregion

    #region Price Validation

    [Fact]
    public void Given_NegativeMaxPrice_When_Validate_Then_ShouldFail()
    {
        var request = CreateValidRequest();
        request.MaxPrice = -1;

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.MaxPrice);
    }

    [Fact]
    public void Given_NegativeMinPrice_When_Validate_Then_ShouldFail()
    {
        var request = CreateValidRequest();
        request.MinPrice = -1;

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.MinPrice);
    }

    [Fact]
    public void Given_MinPriceGreaterThanMaxPrice_When_Validate_Then_ShouldFail()
    {
        var request = CreateValidRequest();
        request.MinPrice = 1000;
        request.MaxPrice = 500;

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.MinPrice);
    }

    [Fact]
    public void Given_ValidPriceRange_When_Validate_Then_ShouldPass()
    {
        var request = CreateValidRequest();
        request.MinPrice = 100;
        request.MaxPrice = 500;

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.MinPrice);
        result.ShouldNotHaveValidationErrorFor(x => x.MaxPrice);
    }

    [Fact]
    public void Given_OnlyMinPrice_When_Validate_Then_ShouldPass()
    {
        var request = CreateValidRequest();
        request.MinPrice = 100;
        request.MaxPrice = null;

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.MinPrice);
    }

    [Fact]
    public void Given_OnlyMaxPrice_When_Validate_Then_ShouldPass()
    {
        var request = CreateValidRequest();
        request.MinPrice = null;
        request.MaxPrice = 500;

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.MaxPrice);
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

    [Fact]
    public void Given_AllFieldsPopulated_When_Validate_Then_ShouldPass()
    {
        var request = new SearchListingsRequest
        {
            Page = 1,
            PageSize = 20,
            City = "Bucharest",
            Area = "Sector 1",
            MinPrice = 200,
            MaxPrice = 800,
            AvailableFrom = DateTime.UtcNow.AddDays(7),
            Amenities = new List<string> { "WiFi", "Parking" },
            OwnerId = Guid.NewGuid(),
            ApprovalStatus = ListingApprovalStatus.Approved
        };

        var result = _validator.TestValidate(request);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}
