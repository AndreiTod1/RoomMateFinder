using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.RoomListings.GetListingById;
using RoomMate_Finder.Features.RoomListings.SearchListings;
using RoomMate_Finder.Test.Helpers;

namespace RoomMate_Finder.Test.Handlers;

#region Get Listing By Id Handler Tests

public class GetListingByIdHandlerTests
{
    private static Profile CreateTestProfile(Guid? id = null, string name = "Test User")
    {
        return new Profile
        {
            Id = id ?? Guid.NewGuid(),
            Email = $"test{Guid.NewGuid():N}@test.com",
            PasswordHash = "hashed",
            FullName = name,
            Age = 25,
            Gender = "Male",
            University = "Test Uni",
            Bio = "Bio",
            Lifestyle = "quiet",
            Interests = "music",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
    }

    private static RoomListing CreateTestListing(Guid ownerId, string title = "Test Listing")
    {
        return new RoomListing
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Title = title,
            Description = "Description",
            City = "Cluj-Napoca",
            Area = "Centru",
            Price = 300,
            AvailableFrom = DateTime.UtcNow.AddDays(30),
            Amenities = "WiFi, Balcony",
            IsActive = true,
            ApprovalStatus = ListingApprovalStatus.Approved,
            ImagePaths = "/images/test.jpg",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Given_ListingNotFound_When_HandleIsCalled_Then_ReturnsNull()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var handler = new GetListingByIdHandler(context);
        var request = new GetListingByIdRequest(Guid.NewGuid());

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_ListingExists_When_HandleIsCalled_Then_ReturnsListingDetails()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var owner = CreateTestProfile(name: "Owner");
        context.Profiles.Add(owner);
        
        var listing = CreateTestListing(owner.Id, "Beautiful Room");
        listing.Owner = owner;
        context.RoomListings.Add(listing);
        await context.SaveChangesAsync();

        var handler = new GetListingByIdHandler(context);
        var request = new GetListingByIdRequest(listing.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Beautiful Room");
        result.OwnerFullName.Should().Be("Owner");
        result.City.Should().Be("Cluj-Napoca");
        result.Amenities.Should().Contain("WiFi");
    }
}

#endregion

#region Search Listings Handler Tests

public class SearchListingsHandlerTests
{
    private static Profile CreateTestProfile(Guid? id = null, string name = "Test User")
    {
        return new Profile
        {
            Id = id ?? Guid.NewGuid(),
            Email = $"test{Guid.NewGuid():N}@test.com",
            PasswordHash = "hashed",
            FullName = name,
            Age = 25,
            Gender = "Male",
            University = "Test Uni",
            Bio = "Bio",
            Lifestyle = "quiet",
            Interests = "music",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
    }

    private static RoomListing CreateTestListing(Profile owner, string city, decimal price, bool isActive = true, ListingApprovalStatus status = ListingApprovalStatus.Approved)
    {
        return new RoomListing
        {
            Id = Guid.NewGuid(),
            OwnerId = owner.Id,
            Owner = owner,
            Title = $"Room in {city}",
            Description = "Description",
            City = city,
            Area = "Center",
            Price = price,
            AvailableFrom = DateTime.UtcNow.AddDays(30),
            Amenities = "WiFi",
            IsActive = isActive,
            ApprovalStatus = status,
            ImagePaths = "",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Given_NoListings_When_HandleIsCalled_Then_ReturnsEmptyResponse()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var handler = new SearchListingsHandler(context);
        var request = new SearchListingsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Listings.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Given_ListingsExist_When_HandleIsCalled_Then_ReturnsOnlyApprovedActiveListings()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var owner = CreateTestProfile(name: "Owner");
        context.Profiles.Add(owner);
        
        context.RoomListings.AddRange(
            CreateTestListing(owner, "Cluj", 300, isActive: true, status: ListingApprovalStatus.Approved),
            CreateTestListing(owner, "Brasov", 400, isActive: true, status: ListingApprovalStatus.Pending),
            CreateTestListing(owner, "Sibiu", 500, isActive: false, status: ListingApprovalStatus.Approved)
        );
        await context.SaveChangesAsync();

        var handler = new SearchListingsHandler(context);
        var request = new SearchListingsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Listings.Should().HaveCount(1); // Only approved and active
        result.Listings[0].City.Should().Be("Cluj");
    }

    [Fact]
    public async Task Given_CityFilter_When_HandleIsCalled_Then_ReturnsFilteredResults()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var owner = CreateTestProfile(name: "Owner");
        context.Profiles.Add(owner);
        
        context.RoomListings.AddRange(
            CreateTestListing(owner, "Cluj-Napoca", 300),
            CreateTestListing(owner, "Bucuresti", 400)
        );
        await context.SaveChangesAsync();

        var handler = new SearchListingsHandler(context);
        var request = new SearchListingsRequest { City = "Cluj-Napoca" };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Listings.Should().HaveCount(1);
        result.Listings[0].City.Should().Be("Cluj-Napoca");
    }

    [Fact]
    public async Task Given_PriceFilter_When_HandleIsCalled_Then_ReturnsFilteredResults()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var owner = CreateTestProfile(name: "Owner");
        context.Profiles.Add(owner);
        
        context.RoomListings.AddRange(
            CreateTestListing(owner, "Cluj", 200),
            CreateTestListing(owner, "Cluj", 350),
            CreateTestListing(owner, "Cluj", 500)
        );
        await context.SaveChangesAsync();

        var handler = new SearchListingsHandler(context);
        var request = new SearchListingsRequest { MinPrice = 250, MaxPrice = 400 };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Listings.Should().HaveCount(1);
        result.Listings[0].Price.Should().Be(350);
    }

    [Fact]
    public async Task Given_Pagination_When_HandleIsCalled_Then_ReturnsPaginatedResults()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var owner = CreateTestProfile(name: "Owner");
        context.Profiles.Add(owner);
        
        for (int i = 1; i <= 15; i++)
        {
            context.RoomListings.Add(CreateTestListing(owner, $"City{i}", 100 + i));
        }
        await context.SaveChangesAsync();

        var handler = new SearchListingsHandler(context);
        var request = new SearchListingsRequest { Page = 1, PageSize = 5 };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Listings.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(5);
    }
}

#endregion
