using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.RoomListings.CreateListing;
using RoomMate_Finder.Infrastructure.Persistence;
using RoomMate_Finder.Test.Helpers;

namespace RoomMate_Finder.Test.Handlers.RoomListings;

public class CreateListingHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;

    public CreateListingHandlerTests()
    {
        _dbContext = DbContextHelper.CreateInMemoryDbContext();
        _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
        _mockWebHostEnvironment.Setup(x => x.WebRootPath).Returns("/wwwroot");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_ValidListing_CreatesListingSuccessfully()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var owner = new Profile
        {
            Id = ownerId,
            Email = "owner@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Property Owner",
            Bio = "Property owner with multiple listings",
            Age = 35,
            Gender = "Male",
            University = "Business University",
            Lifestyle = "Professional",
            Interests = "Real Estate, Investment",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Profiles.AddAsync(owner);
        await _dbContext.SaveChangesAsync();

        var request = new CreateListingRequest
        {
            Title = "Cozy 2-bedroom apartment",
            Description = "Beautiful apartment in the city center with modern amenities",
            City = "Bucharest",
            Area = "Old Town",
            Price = 800.50m,
            AvailableFrom = DateTime.UtcNow.AddDays(30),
            Amenities = new List<string> { "WiFi", "Parking", "Air Conditioning" },
            OwnerId = ownerId
        };

        var command = new CreateListingWithImagesCommand(request, new List<IFormFile>());
        var handler = new CreateListingHandler(_dbContext, _mockWebHostEnvironment.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();

        var createdListing = await _dbContext.RoomListings.FindAsync(result.Id);
        createdListing.Should().NotBeNull();
        createdListing!.Title.Should().Be("Cozy 2-bedroom apartment");
        createdListing.Description.Should().Be("Beautiful apartment in the city center with modern amenities");
        createdListing.City.Should().Be("Bucharest");
        createdListing.Area.Should().Be("Old Town");
        createdListing.Price.Should().Be(800.50m);
        createdListing.OwnerId.Should().Be(ownerId);
        createdListing.IsActive.Should().BeTrue();
        createdListing.Amenities.Should().Be("WiFi,Parking,Air Conditioning");
    }

    [Fact]
    public async Task Handle_NonExistentOwner_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentOwnerId = Guid.NewGuid();

        var request = new CreateListingRequest
        {
            Title = "Test Listing",
            Description = "Test Description",
            City = "Test City",
            Area = "Test Area",
            Price = 500m,
            AvailableFrom = DateTime.UtcNow.AddDays(30),
            Amenities = new List<string> { "WiFi" },
            OwnerId = nonExistentOwnerId
        };

        var command = new CreateListingWithImagesCommand(request, new List<IFormFile>());
        var handler = new CreateListingHandler(_dbContext, _mockWebHostEnvironment.Object);

        // Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Owner profile not found*");
    }

    [Fact]
    public async Task Handle_MinimalValidData_CreatesListing()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var owner = new Profile
        {
            Id = ownerId,
            Email = "minimal@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Minimal Owner",
            Bio = "Basic profile",
            Age = 25,
            Gender = "Female",
            University = "University",
            Lifestyle = "Active",
            Interests = "Housing",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Profiles.AddAsync(owner);
        await _dbContext.SaveChangesAsync();

        var request = new CreateListingRequest
        {
            Title = "Room",
            Description = "Basic room",
            City = "City",
            Area = "Area",
            Price = 100m,
            AvailableFrom = DateTime.UtcNow.AddDays(1),
            Amenities = new List<string>(),
            OwnerId = ownerId
        };

        var command = new CreateListingWithImagesCommand(request, new List<IFormFile>());
        var handler = new CreateListingHandler(_dbContext, _mockWebHostEnvironment.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        
        var createdListing = await _dbContext.RoomListings.FindAsync(result.Id);
        createdListing.Should().NotBeNull();
        createdListing!.Title.Should().Be("Room");
        createdListing.Price.Should().Be(100m);
        createdListing.Amenities.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_LongDescriptionAndManyAmenities_CreatesListingSuccessfully()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var owner = new Profile
        {
            Id = ownerId,
            Email = "detailed@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Detailed Owner",
            Bio = "Detailed profile",
            Age = 40,
            Gender = "Male",
            University = "University",
            Lifestyle = "Professional",
            Interests = "Real Estate",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Profiles.AddAsync(owner);
        await _dbContext.SaveChangesAsync();

        var longDescription = new string('A', 1500); // Long description
        var manyAmenities = new List<string> 
        { 
            "WiFi", "Parking", "Air Conditioning", "Heating", "Kitchen", 
            "Bathroom", "Balcony", "Garden", "Gym", "Pool", "Security"
        };

        var request = new CreateListingRequest
        {
            Title = "Luxury Apartment with All Amenities",
            Description = longDescription,
            City = "Bucharest",
            Area = "Herastrau",
            Price = 1500.75m,
            AvailableFrom = DateTime.UtcNow.AddDays(60),
            Amenities = manyAmenities,
            OwnerId = ownerId
        };

        var command = new CreateListingWithImagesCommand(request, new List<IFormFile>());
        var handler = new CreateListingHandler(_dbContext, _mockWebHostEnvironment.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();

        var createdListing = await _dbContext.RoomListings.FindAsync(result.Id);
        createdListing.Should().NotBeNull();
        createdListing!.Description.Should().Be(longDescription);
        createdListing.Price.Should().Be(1500.75m);
        createdListing.Amenities.Should().Contain("WiFi,Parking,Air Conditioning,Heating,Kitchen,Bathroom,Balcony,Garden,Gym,Pool,Security");
    }

    [Fact]
    public async Task Handle_FutureAvailableDate_CreatesListingSuccessfully()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var owner = new Profile
        {
            Id = ownerId,
            Email = "future@example.com",
            PasswordHash = "hashedpassword",
            FullName = "Future Owner",
            Bio = "Planning ahead",
            Age = 30,
            Gender = "Other",
            University = "University",
            Lifestyle = "Balanced",
            Interests = "Planning",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Profiles.AddAsync(owner);
        await _dbContext.SaveChangesAsync();

        var futureDate = DateTime.UtcNow.AddDays(365); // One year from now

        var request = new CreateListingRequest
        {
            Title = "Future Available Apartment",
            Description = "Available next year",
            City = "Cluj",
            Area = "Center",
            Price = 900m,
            AvailableFrom = futureDate,
            Amenities = new List<string> { "WiFi", "Heating" },
            OwnerId = ownerId
        };

        var command = new CreateListingWithImagesCommand(request, new List<IFormFile>());
        var handler = new CreateListingHandler(_dbContext, _mockWebHostEnvironment.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();

        var createdListing = await _dbContext.RoomListings.FindAsync(result.Id);
        createdListing.Should().NotBeNull();
        createdListing!.AvailableFrom.Should().BeCloseTo(futureDate, TimeSpan.FromMinutes(1));
    }
}
