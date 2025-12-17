using FluentAssertions;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.RoomListings.UpdateListing;
using RoomMate_Finder.Infrastructure.Persistence;
using RoomMate_Finder.Test.Helpers;

namespace RoomMate_Finder.Test.Handlers.RoomListings;

public class UpdateListingHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;

    public UpdateListingHandlerTests()
    {
        _dbContext = DbContextHelper.CreateInMemoryDbContext();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_ValidUpdate_UpdatesListingSuccessfully()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var owner = new Profile
        {
            Id = ownerId,
            Email = "owner@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Property Owner",
            Bio = "Property owner",
            Age = 35,
            Gender = "Female",
            University = "Business School",
            Lifestyle = "Professional",
            Interests = "Real Estate",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var originalListing = new RoomListing
        {
            Id = listingId,
            OwnerId = ownerId,
            Title = "Original Title",
            Description = "Original description",
            City = "Original City",
            Area = "Original Area",
            Price = 500m,
            AvailableFrom = DateTime.UtcNow.AddDays(10),
            Amenities = "WiFi,Parking",
            ImagePaths = "/images/original.jpg",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        await _dbContext.Profiles.AddAsync(owner);
        await _dbContext.RoomListings.AddAsync(originalListing);
        await _dbContext.SaveChangesAsync();

        var request = new UpdateListingRequest
        {
            Id = listingId,
            OwnerId = ownerId,
            Title = "Updated Modern Apartment",
            Description = "Newly renovated apartment with updated amenities",
            City = "Bucharest",
            Area = "City Center",
            Price = 750m,
            AvailableFrom = DateTime.UtcNow.AddDays(20),
            Amenities = new List<string> { "WiFi", "Parking", "Air Conditioning", "Balcony" },
            IsActive = true
        };

        var handler = new UpdateListingHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(listingId);

        var updatedListing = await _dbContext.RoomListings.FindAsync(listingId);
        updatedListing.Should().NotBeNull();
        updatedListing!.Title.Should().Be("Updated Modern Apartment");
        updatedListing.Description.Should().Be("Newly renovated apartment with updated amenities");
        updatedListing.City.Should().Be("Bucharest");
        updatedListing.Area.Should().Be("City Center");
        updatedListing.Price.Should().Be(750m);
        updatedListing.AvailableFrom.Should().BeCloseTo(DateTime.UtcNow.AddDays(20), TimeSpan.FromMinutes(1));
        updatedListing.Amenities.Should().Be("WiFi,Parking,Air Conditioning,Balcony");
        updatedListing.IsActive.Should().BeTrue();
        updatedListing.UpdatedAt.Should().NotBeNull();
        updatedListing.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Handle_NonExistentListing_ReturnsNull()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var nonExistentListingId = Guid.NewGuid();

        var owner = new Profile
        {
            Id = ownerId,
            Email = "owner@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Property Owner",
            Bio = "Property owner",
            Age = 35,
            Gender = "Male",
            University = "University",
            Lifestyle = "Professional",
            Interests = "Real Estate",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        await _dbContext.Profiles.AddAsync(owner);
        await _dbContext.SaveChangesAsync();

        var request = new UpdateListingRequest
        {
            Id = nonExistentListingId,
            OwnerId = ownerId,
            Title = "Updated Title",
            Description = "Updated description",
            City = "City",
            Area = "Area",
            Price = 600m,
            AvailableFrom = DateTime.UtcNow.AddDays(15),
            Amenities = new List<string> { "WiFi" },
            IsActive = true
        };

        var handler = new UpdateListingHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UnauthorizedOwner_ReturnsNull()
    {
        // Arrange
        var realOwnerId = Guid.NewGuid();
        var fakeOwnerId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var realOwner = new Profile
        {
            Id = realOwnerId,
            Email = "realowner@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Real Owner",
            Bio = "Real property owner",
            Age = 35,
            Gender = "Female",
            University = "University",
            Lifestyle = "Professional",
            Interests = "Real Estate",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var fakeOwner = new Profile
        {
            Id = fakeOwnerId,
            Email = "fakeowner@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Fake Owner",
            Bio = "Not the real owner",
            Age = 30,
            Gender = "Male",
            University = "University",
            Lifestyle = "Active",
            Interests = "Hacking",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };

        var listing = new RoomListing
        {
            Id = listingId,
            OwnerId = realOwnerId,
            Title = "Original Title",
            Description = "Original description",
            City = "Original City",
            Area = "Original Area",
            Price = 500m,
            AvailableFrom = DateTime.UtcNow.AddDays(10),
            Amenities = "WiFi",
            ImagePaths = "",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        await _dbContext.Profiles.AddRangeAsync(realOwner, fakeOwner);
        await _dbContext.RoomListings.AddAsync(listing);
        await _dbContext.SaveChangesAsync();

        var request = new UpdateListingRequest
        {
            Id = listingId,
            OwnerId = fakeOwnerId, // Wrong owner trying to update
            Title = "Malicious Update",
            Description = "Hacked description",
            City = "Hacker City",
            Area = "Dark Web",
            Price = 1m,
            AvailableFrom = DateTime.UtcNow.AddDays(1),
            Amenities = new List<string> { "Free Hacks" },
            IsActive = true
        };

        var handler = new UpdateListingHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        // Verify original listing was not changed
        var unchangedListing = await _dbContext.RoomListings.FindAsync(listingId);
        unchangedListing.Should().NotBeNull();
        unchangedListing!.Title.Should().Be("Original Title");
        unchangedListing.OwnerId.Should().Be(realOwnerId);
    }

    [Fact]
    public async Task Handle_DeactivateListing_UpdatesIsActiveField()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var owner = new Profile
        {
            Id = ownerId,
            Email = "owner@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Property Owner",
            Bio = "Property owner",
            Age = 35,
            Gender = "Other",
            University = "University",
            Lifestyle = "Professional",
            Interests = "Real Estate",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var activeListing = new RoomListing
        {
            Id = listingId,
            OwnerId = ownerId,
            Title = "Active Listing",
            Description = "Currently active listing",
            City = "Cluj",
            Area = "Center",
            Price = 600m,
            AvailableFrom = DateTime.UtcNow.AddDays(10),
            Amenities = "WiFi,Heating",
            ImagePaths = "/images/active.jpg",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        await _dbContext.Profiles.AddAsync(owner);
        await _dbContext.RoomListings.AddAsync(activeListing);
        await _dbContext.SaveChangesAsync();

        var request = new UpdateListingRequest
        {
            Id = listingId,
            OwnerId = ownerId,
            Title = "Active Listing",
            Description = "Currently active listing",
            City = "Cluj",
            Area = "Center",
            Price = 600m,
            AvailableFrom = DateTime.UtcNow.AddDays(10),
            Amenities = new List<string> { "WiFi", "Heating" },
            IsActive = false // Deactivating the listing
        };

        var handler = new UpdateListingHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(listingId);

        var deactivatedListing = await _dbContext.RoomListings.FindAsync(listingId);
        deactivatedListing.Should().NotBeNull();
        deactivatedListing!.IsActive.Should().BeFalse();
        deactivatedListing.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_UpdatePriceOnly_UpdatesOnlyPrice()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var owner = new Profile
        {
            Id = ownerId,
            Email = "owner@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Property Owner",
            Bio = "Property owner",
            Age = 35,
            Gender = "Male",
            University = "University",
            Lifestyle = "Professional",
            Interests = "Real Estate",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var originalListing = new RoomListing
        {
            Id = listingId,
            OwnerId = ownerId,
            Title = "Stable Title",
            Description = "Stable description",
            City = "Stable City",
            Area = "Stable Area",
            Price = 500m,
            AvailableFrom = DateTime.UtcNow.AddDays(10),
            Amenities = "WiFi,Parking",
            ImagePaths = "/images/stable.jpg",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        await _dbContext.Profiles.AddAsync(owner);
        await _dbContext.RoomListings.AddAsync(originalListing);
        await _dbContext.SaveChangesAsync();

        var request = new UpdateListingRequest
        {
            Id = listingId,
            OwnerId = ownerId,
            Title = "Stable Title",
            Description = "Stable description",
            City = "Stable City",
            Area = "Stable Area",
            Price = 700m, // Only price changed
            AvailableFrom = DateTime.UtcNow.AddDays(10),
            Amenities = new List<string> { "WiFi", "Parking" },
            IsActive = true
        };

        var handler = new UpdateListingHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(listingId);

        var updatedListing = await _dbContext.RoomListings.FindAsync(listingId);
        updatedListing.Should().NotBeNull();
        updatedListing!.Price.Should().Be(700m);
        updatedListing.Title.Should().Be("Stable Title");
        updatedListing.Description.Should().Be("Stable description");
        updatedListing.City.Should().Be("Stable City");
        updatedListing.Area.Should().Be("Stable Area");
        updatedListing.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_RemoveAllAmenities_UpdatesAmenitiesField()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var owner = new Profile
        {
            Id = ownerId,
            Email = "owner@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Property Owner",
            Bio = "Property owner",
            Age = 35,
            Gender = "Female",
            University = "University",
            Lifestyle = "Professional",
            Interests = "Real Estate",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var listingWithAmenities = new RoomListing
        {
            Id = listingId,
            OwnerId = ownerId,
            Title = "Listing With Amenities",
            Description = "Previously had amenities",
            City = "Timisoara",
            Area = "Center",
            Price = 400m,
            AvailableFrom = DateTime.UtcNow.AddDays(10),
            Amenities = "WiFi,Parking,Pool,Gym",
            ImagePaths = "",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        await _dbContext.Profiles.AddAsync(owner);
        await _dbContext.RoomListings.AddAsync(listingWithAmenities);
        await _dbContext.SaveChangesAsync();

        var request = new UpdateListingRequest
        {
            Id = listingId,
            OwnerId = ownerId,
            Title = "Listing With Amenities",
            Description = "Previously had amenities",
            City = "Timisoara",
            Area = "Center",
            Price = 400m,
            AvailableFrom = DateTime.UtcNow.AddDays(10),
            Amenities = new List<string>(), // Remove all amenities
            IsActive = true
        };

        var handler = new UpdateListingHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(listingId);

        var updatedListing = await _dbContext.RoomListings.FindAsync(listingId);
        updatedListing.Should().NotBeNull();
        updatedListing!.Amenities.Should().BeEmpty();
    }
}
