using FluentAssertions;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.RoomListings.GetListingById;
using RoomMate_Finder.Infrastructure.Persistence;
using RoomMate_Finder.Test.Helpers;

namespace RoomMate_Finder.Test.Handlers.RoomListings;

public class GetListingByIdHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;

    public GetListingByIdHandlerTests()
    {
        _dbContext = DbContextHelper.CreateInMemoryDbContext();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_ExistingListing_ReturnsListingDetails()
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
            Bio = "Experienced property owner",
            Age = 45,
            Gender = "Female",
            University = "Business School",
            Lifestyle = "Professional",
            Interests = "Real Estate, Business",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-100)
        };

        var listing = new RoomListing
        {
            Id = listingId,
            OwnerId = ownerId,
            Title = "Modern Studio Apartment",
            Description = "A beautiful studio apartment in the heart of the city with all modern amenities",
            City = "Bucharest",
            Area = "Old Town",
            Price = 650.00m,
            AvailableFrom = DateTime.UtcNow.AddDays(15),
            Amenities = "WiFi,Parking,Air Conditioning,Kitchen,Bathroom",
            ImagePaths = "/images/listing1.jpg,/images/listing2.jpg",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        await _dbContext.Profiles.AddAsync(owner);
        await _dbContext.RoomListings.AddAsync(listing);
        await _dbContext.SaveChangesAsync();

        var request = new GetListingByIdRequest(listingId);
        var handler = new GetListingByIdHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(listingId);
        result.Title.Should().Be("Modern Studio Apartment");
        result.Description.Should().Be("A beautiful studio apartment in the heart of the city with all modern amenities");
        result.City.Should().Be("Bucharest");
        result.Area.Should().Be("Old Town");
        result.Price.Should().Be(650.00m);
        result.AvailableFrom.Should().BeCloseTo(DateTime.UtcNow.AddDays(15), TimeSpan.FromMinutes(1));
        result.Amenities.Should().Contain("WiFi");
        result.Amenities.Should().Contain("Parking");
        result.Amenities.Should().Contain("Air Conditioning");
        result.ImagePaths.Should().Contain("/images/listing1.jpg");
        result.ImagePaths.Should().Contain("/images/listing2.jpg");
        result.IsActive.Should().BeTrue();
        result.OwnerFullName.Should().Be("Property Owner");
        // Owner email is not in response
    }

    [Fact]
    public async Task Handle_NonExistentListing_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new GetListingByIdRequest(nonExistentId);
        var handler = new GetListingByIdHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_InactiveListing_ReturnsListingDetails()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var owner = new Profile
        {
            Id = ownerId,
            Email = "inactive@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Inactive Owner",
            Bio = "Owner with inactive listing",
            Age = 30,
            Gender = "Male",
            University = "University",
            Lifestyle = "Calm",
            Interests = "Real Estate",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-50)
        };

        var inactiveListing = new RoomListing
        {
            Id = listingId,
            OwnerId = ownerId,
            Title = "Inactive Listing",
            Description = "This listing is no longer active",
            City = "Cluj",
            Area = "Center",
            Price = 500.00m,
            AvailableFrom = DateTime.UtcNow.AddDays(-10),
            Amenities = "WiFi,Heating",
            ImagePaths = "",
            IsActive = false, // Inactive listing
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        await _dbContext.Profiles.AddAsync(owner);
        await _dbContext.RoomListings.AddAsync(inactiveListing);
        await _dbContext.SaveChangesAsync();

        var request = new GetListingByIdRequest(listingId);
        var handler = new GetListingByIdHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(listingId);
        result.Title.Should().Be("Inactive Listing");
        result.IsActive.Should().BeFalse();
        result.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ListingWithoutImages_ReturnsListingWithEmptyImagePaths()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var owner = new Profile
        {
            Id = ownerId,
            Email = "noimage@example.com",
            PasswordHash = "hashedpassword",
            FullName = "No Image Owner",
            Bio = "Owner without images",
            Age = 25,
            Gender = "Other",
            University = "University",
            Lifestyle = "Active",
            Interests = "Housing",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-20)
        };

        var listingNoImages = new RoomListing
        {
            Id = listingId,
            OwnerId = ownerId,
            Title = "Basic Room",
            Description = "Simple room without photos",
            City = "Timisoara",
            Area = "Student Area",
            Price = 300.00m,
            AvailableFrom = DateTime.UtcNow.AddDays(7),
            Amenities = "WiFi",
            ImagePaths = "", // No images
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };

        await _dbContext.Profiles.AddAsync(owner);
        await _dbContext.RoomListings.AddAsync(listingNoImages);
        await _dbContext.SaveChangesAsync();

        var request = new GetListingByIdRequest(listingId);
        var handler = new GetListingByIdHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(listingId);
        result.Title.Should().Be("Basic Room");
        result.ImagePaths.Should().BeEmpty();
        result.Price.Should().Be(300.00m);
    }

    [Fact]
    public async Task Handle_ListingWithoutAmenities_ReturnsListingWithEmptyAmenities()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        var owner = new Profile
        {
            Id = ownerId,
            Email = "basic@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Basic Owner",
            Bio = "Basic accommodations",
            Age = 35,
            Gender = "Male",
            University = "University",
            Lifestyle = "Simple",
            Interests = "Minimalism",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        var basicListing = new RoomListing
        {
            Id = listingId,
            OwnerId = ownerId,
            Title = "Very Basic Room",
            Description = "Basic room with no amenities",
            City = "Iasi",
            Area = "Suburbs",
            Price = 200.00m,
            AvailableFrom = DateTime.UtcNow.AddDays(5),
            Amenities = "", // No amenities
            ImagePaths = "/images/basic.jpg",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-7)
        };

        await _dbContext.Profiles.AddAsync(owner);
        await _dbContext.RoomListings.AddAsync(basicListing);
        await _dbContext.SaveChangesAsync();

        var request = new GetListingByIdRequest(listingId);
        var handler = new GetListingByIdHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(listingId);
        result.Title.Should().Be("Very Basic Room");
        result.Amenities.Should().BeEmpty();
        result.Price.Should().Be(200.00m);
        result.OwnerFullName.Should().Be("Basic Owner");
    }

    [Fact]
    public async Task Handle_MultipleListings_ReturnsCorrectListing()
    {
        // Arrange
        var owner1Id = Guid.NewGuid();
        var owner2Id = Guid.NewGuid();
        var listing1Id = Guid.NewGuid();
        var listing2Id = Guid.NewGuid();

        var owner1 = new Profile
        {
            Id = owner1Id,
            Email = "owner1@example.com",
            PasswordHash = "hash1",
            FullName = "First Owner",
            Bio = "First owner",
            Age = 30,
            Gender = "Male",
            University = "University 1",
            Lifestyle = "Active",
            Interests = "Sports",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        var owner2 = new Profile
        {
            Id = owner2Id,
            Email = "owner2@example.com",
            PasswordHash = "hash2",
            FullName = "Second Owner",
            Bio = "Second owner",
            Age = 35,
            Gender = "Female",
            University = "University 2",
            Lifestyle = "Calm",
            Interests = "Reading",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-60)
        };

        var listing1 = new RoomListing
        {
            Id = listing1Id,
            OwnerId = owner1Id,
            Title = "First Listing",
            Description = "Description 1",
            City = "City 1",
            Area = "Area 1",
            Price = 400m,
            AvailableFrom = DateTime.UtcNow.AddDays(10),
            Amenities = "WiFi",
            ImagePaths = "",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        var listing2 = new RoomListing
        {
            Id = listing2Id,
            OwnerId = owner2Id,
            Title = "Second Listing",
            Description = "Description 2",
            City = "City 2",
            Area = "Area 2",
            Price = 600m,
            AvailableFrom = DateTime.UtcNow.AddDays(20),
            Amenities = "WiFi,Parking",
            ImagePaths = "/images/second.jpg",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        await _dbContext.Profiles.AddRangeAsync(owner1, owner2);
        await _dbContext.RoomListings.AddRangeAsync(listing1, listing2);
        await _dbContext.SaveChangesAsync();

        var request = new GetListingByIdRequest(listing2Id);
        var handler = new GetListingByIdHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(listing2Id);
        result.Title.Should().Be("Second Listing");
        result.Price.Should().Be(600m);
        result.OwnerFullName.Should().Be("Second Owner");
        // Owner email is not in response
    }
}
