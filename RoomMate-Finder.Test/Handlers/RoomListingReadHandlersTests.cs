using FluentAssertions;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.RoomListings.GetListingById;
using RoomMate_Finder.Features.RoomListings.SearchListings;
using RoomMate_Finder.Test.Helpers;

namespace RoomMate_Finder.Test.Handlers;

public class RoomListingReadHandlersTests
{
    private static RoomListing CreateListing(Guid ownerId, string title, decimal price, string city, string amenities, bool isActive = true, ListingApprovalStatus status = ListingApprovalStatus.Approved)
    {
        return new RoomListing
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Title = title,
            Description = "Description",
            City = city,
            Area = "Center",
            Price = price,
            AvailableFrom = DateTime.UtcNow,
            Amenities = amenities,
            IsActive = isActive,
            ApprovalStatus = status,
            CreatedAt = DateTime.UtcNow
        };
    }

    #region SearchListingsHandler Tests

    [Fact]
    public async Task Given_NoFilters_When_SearchListingsIsCalled_Then_ReturnsApprovedAndActiveListings()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var owner = new Profile { Id = Guid.NewGuid(), FullName = "Owner", Email = "o@t.com", Age=20, Gender="M", CreatedAt=DateTime.UtcNow, PasswordHash="h" };
        context.Profiles.Add(owner);

        var l1 = CreateListing(owner.Id, "L1", 100, "City1", "Wifi");
        var l2 = CreateListing(owner.Id, "L2", 200, "City2", "Wifi");
        var l3 = CreateListing(owner.Id, "L3", 300, "City1", "Wifi", isActive: false); // Inactive
        var l4 = CreateListing(owner.Id, "L4", 400, "City1", "Wifi", status: ListingApprovalStatus.Pending); // Pending

        context.RoomListings.AddRange(l1, l2, l3, l4);
        await context.SaveChangesAsync();

        var handler = new SearchListingsHandler(context);
        var request = new SearchListingsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Listings.Should().HaveCount(2); // Only l1 and l2
        result.TotalCount.Should().Be(2);
        result.Listings.Should().Contain(l => l.Title == "L1");
        result.Listings.Should().Contain(l => l.Title == "L2");
    }

    [Fact]
    public async Task Given_CityFilter_When_SearchListingsIsCalled_Then_ReturnsMatchingCity()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var owner = new Profile { Id = Guid.NewGuid(), FullName = "Owner", Email = "o@t.com", Age=20, Gender="M", CreatedAt=DateTime.UtcNow, PasswordHash="h" };
        context.Profiles.Add(owner);

        var l1 = CreateListing(owner.Id, "L1", 100, "Bucharest", "Wifi");
        var l2 = CreateListing(owner.Id, "L2", 200, "Cluj", "Wifi");
        
        context.RoomListings.AddRange(l1, l2);
        await context.SaveChangesAsync();

        var handler = new SearchListingsHandler(context);
        var request = new SearchListingsRequest { City = "Bucharest" };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Listings.Should().HaveCount(1);
        result.Listings.First().Title.Should().Be("L1");
    }

    [Fact]
    public async Task Given_PriceRangeFilter_When_SearchListingsIsCalled_Then_ReturnsListingsInRange()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var owner = new Profile { Id = Guid.NewGuid(), FullName = "Owner", Email = "o@t.com", Age=20, Gender="M", CreatedAt=DateTime.UtcNow, PasswordHash="h" };
        context.Profiles.Add(owner);

        var l1 = CreateListing(owner.Id, "Cheap", 100, "City", "Wifi");
        var l2 = CreateListing(owner.Id, "Medium", 300, "City", "Wifi");
        var l3 = CreateListing(owner.Id, "Expensive", 600, "City", "Wifi");
        
        context.RoomListings.AddRange(l1, l2, l3);
        await context.SaveChangesAsync();

        var handler = new SearchListingsHandler(context);
        var request = new SearchListingsRequest { MinPrice = 200, MaxPrice = 500 };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Listings.Should().HaveCount(1);
        result.Listings.First().Title.Should().Be("Medium");
    }

    [Fact]
    public async Task Given_AmenitiesFilter_When_SearchListingsIsCalled_Then_ReturnsListingsWithAmenities()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var owner = new Profile { Id = Guid.NewGuid(), FullName = "Owner", Email = "o@t.com", Age=20, Gender="M", CreatedAt=DateTime.UtcNow, PasswordHash="h" };
        context.Profiles.Add(owner);

        var l1 = CreateListing(owner.Id, "WifiOnly", 100, "City", "Wifi");
        var l2 = CreateListing(owner.Id, "WifiAndAC", 200, "City", "Wifi, AC");
        var l3 = CreateListing(owner.Id, "ParkingOnly", 300, "City", "Parking");
        
        context.RoomListings.AddRange(l1, l2, l3);
        await context.SaveChangesAsync();

        var handler = new SearchListingsHandler(context);
        var request = new SearchListingsRequest { Amenities = new List<string> { "AC" } };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Listings.Should().HaveCount(1);
        result.Listings.First().Title.Should().Be("WifiAndAC");
    }

    [Fact]
    public async Task Given_InactiveFilter_When_SearchListingsIsCalled_Then_ReturnsInactiveToo()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var owner = new Profile { Id = Guid.NewGuid(), FullName = "Owner", Email = "o@t.com", Age=20, Gender="M", CreatedAt=DateTime.UtcNow, PasswordHash="h" };
        context.Profiles.Add(owner);

        var l1 = CreateListing(owner.Id, "Active", 100, "City", "Wifi", isActive: true);
        var l2 = CreateListing(owner.Id, "Inactive", 200, "City", "Wifi", isActive: false);
        
        context.RoomListings.AddRange(l1, l2);
        await context.SaveChangesAsync();

        var handler = new SearchListingsHandler(context);
        var request = new SearchListingsRequest { IncludeInactive = true };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Listings.Should().HaveCount(2);
    }

    #endregion

    #region GetListingByIdHandler Tests

    [Fact]
    public async Task Given_NonExistentId_When_GetListingByIdIsCalled_Then_ReturnsNull()
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
    public async Task Given_ValidId_When_GetListingByIdIsCalled_Then_ReturnsDetailView()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var owner = new Profile { Id = Guid.NewGuid(), FullName = "Owner Name", Email = "o@t.com", Age=20, Gender="M", CreatedAt=DateTime.UtcNow, PasswordHash="h" };
        var listing = CreateListing(owner.Id, "Detail View", 500, "Bucharest", "Wifi, Parking");
        
        context.Profiles.Add(owner);
        context.RoomListings.Add(listing);
        await context.SaveChangesAsync();

        var handler = new GetListingByIdHandler(context);
        var request = new GetListingByIdRequest(listing.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(listing.Id);
        result.Title.Should().Be("Detail View");
        result.OwnerFullName.Should().Be("Owner Name");
        result.Amenities.Should().Contain("Parking");
        result.Amenities.Should().Contain("Wifi");
    }

    #endregion
}
