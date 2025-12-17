using FluentAssertions;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.RoomListings.SearchListings;
using RoomMate_Finder.Infrastructure.Persistence;
using RoomMate_Finder.Test.Helpers;

namespace RoomMate_Finder.Test.Handlers.RoomListings;

public class SearchListingsHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext;

    public SearchListingsHandlerTests()
    {
        _dbContext = DbContextHelper.CreateInMemoryDbContext();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_NoFilters_ReturnsAllActiveListings()
    {
        // Arrange
        await SeedTestDataAsync();

        var request = new SearchListingsRequest();
        var handler = new SearchListingsHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Listings.Should().NotBeEmpty();
        result.Listings.Should().AllSatisfy(l => l.IsActive.Should().BeTrue());
        result.TotalCount.Should().BeGreaterThan(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Handle_FilterByCity_ReturnsListingsInSpecifiedCity()
    {
        // Arrange
        await SeedTestDataAsync();

        var request = new SearchListingsRequest
        {
            City = "Bucharest"
        };
        var handler = new SearchListingsHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Listings.Should().AllSatisfy(l => l.City.Should().Be("Bucharest"));
    }

    [Fact]
    public async Task Handle_FilterByPriceRange_ReturnsListingsWithinRange()
    {
        // Arrange
        await SeedTestDataAsync();

        var request = new SearchListingsRequest
        {
            MinPrice = 400m,
            MaxPrice = 800m
        };
        var handler = new SearchListingsHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Listings.Should().AllSatisfy(l => 
        {
            l.Price.Should().BeGreaterOrEqualTo(400m);
            l.Price.Should().BeLessOrEqualTo(800m);
        });
    }

    [Fact]
    public async Task Handle_FilterByAmenities_ReturnsListingsWithRequiredAmenities()
    {
        // Arrange
        await SeedTestDataAsync();

        var request = new SearchListingsRequest
        {
            Amenities = new List<string> { "WiFi", "Parking" }
        };
        var handler = new SearchListingsHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Listings.Should().AllSatisfy(l => 
        {
            l.Amenities.Should().Contain("WiFi");
            l.Amenities.Should().Contain("Parking");
        });
    }

    [Fact]
    public async Task Handle_FilterByAvailableDate_ReturnsAppropriateListings()
    {
        // Arrange
        await SeedTestDataAsync();

        var targetDate = DateTime.UtcNow.AddDays(30);
        var request = new SearchListingsRequest
        {
            AvailableFrom = targetDate
        };
        var handler = new SearchListingsHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Listings.Should().AllSatisfy(l => 
            l.AvailableFrom.Date.Should().BeOnOrAfter(targetDate.Date));
    }

    [Fact]
    public async Task Handle_FilterByOwner_ReturnsListingsFromSpecificOwner()
    {
        // Arrange
        var ownerId = await SeedTestDataAsync();

        var request = new SearchListingsRequest
        {
            OwnerId = ownerId
        };
        var handler = new SearchListingsHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Listings.Should().AllSatisfy(l => l.OwnerId.Should().Be(ownerId));
    }

    [Fact]
    public async Task Handle_IncludeInactiveTrue_ReturnsAllListings()
    {
        // Arrange
        await SeedTestDataAsync();

        var request = new SearchListingsRequest
        {
            IncludeInactive = true
        };
        var handler = new SearchListingsHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Listings.Should().Contain(l => l.IsActive == false);
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmptyResult()
    {
        // Arrange
        var request = new SearchListingsRequest();
        var handler = new SearchListingsHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Listings.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectPage()
    {
        // Arrange
        await SeedLargeTestDataAsync(); // Add many listings

        var request = new SearchListingsRequest
        {
            Page = 2,
            PageSize = 5
        };
        var handler = new SearchListingsHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.Listings.Should().HaveCountLessOrEqualTo(5);
    }

    [Fact]
    public async Task Handle_MultipleFilters_ReturnsFilteredResults()
    {
        // Arrange
        await SeedTestDataAsync();

        var request = new SearchListingsRequest
        {
            City = "Bucharest",
            MinPrice = 500m,
            MaxPrice = 1500m, // Increased from 1000m to include luxury apartment
            Amenities = new List<string> { "WiFi" },
            AvailableFrom = DateTime.UtcNow.AddDays(40) // Changed from 50 to 40 to include luxury apartment
        };
        var handler = new SearchListingsHandler(_dbContext);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Listings.Should().AllSatisfy(l => 
        {
            l.City.Should().Be("Bucharest");
            l.Price.Should().BeGreaterOrEqualTo(500m);
            l.Price.Should().BeLessOrEqualTo(1500m);
            l.Amenities.Should().Contain("WiFi");
            l.AvailableFrom.Date.Should().BeOnOrAfter(DateTime.UtcNow.AddDays(40).Date);
        });
    }

    private async Task<Guid> SeedTestDataAsync()
    {
        var owner1Id = Guid.NewGuid();
        var owner2Id = Guid.NewGuid();

        var owners = new List<Profile>
        {
            new Profile
            {
                Id = owner1Id,
                Email = "owner1@example.com",
                PasswordHash = "hash1",
                FullName = "Owner One",
                Bio = "First property owner",
                Age = 35,
                Gender = "Male",
                University = "Business University",
                Lifestyle = "Professional",
                Interests = "Real Estate",
                Role = "User",
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            new Profile
            {
                Id = owner2Id,
                Email = "owner2@example.com",
                PasswordHash = "hash2",
                FullName = "Owner Two",
                Bio = "Second property owner",
                Age = 40,
                Gender = "Female",
                University = "Management School",
                Lifestyle = "Calm",
                Interests = "Investment",
                Role = "User",
                CreatedAt = DateTime.UtcNow.AddDays(-90)
            }
        };

        var listings = new List<RoomListing>
        {
            new RoomListing
            {
                Id = Guid.NewGuid(),
                OwnerId = owner1Id,
                Title = "Cozy Apartment Bucharest",
                Description = "Nice apartment in city center",
                City = "Bucharest",
                Area = "Old Town",
                Price = 700m,
                AvailableFrom = DateTime.UtcNow.AddDays(15),
                Amenities = "WiFi,Parking,Air Conditioning",
                ImagePaths = "/images/apt1.jpg",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new RoomListing
            {
                Id = Guid.NewGuid(),
                OwnerId = owner2Id,
                Title = "Modern Studio Cluj",
                Description = "Studio apartment near university",
                City = "Cluj",
                Area = "Center",
                Price = 450m,
                AvailableFrom = DateTime.UtcNow.AddDays(30),
                Amenities = "WiFi,Heating",
                ImagePaths = "/images/studio1.jpg",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new RoomListing
            {
                Id = Guid.NewGuid(),
                OwnerId = owner1Id,
                Title = "Luxury Apartment Bucharest",
                Description = "High-end apartment with all amenities",
                City = "Bucharest",
                Area = "Herastrau",
                Price = 1200m,
                AvailableFrom = DateTime.UtcNow.AddDays(45),
                Amenities = "WiFi,Parking,Pool,Gym,Air Conditioning",
                ImagePaths = "/images/luxury1.jpg,/images/luxury2.jpg",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new RoomListing
            {
                Id = Guid.NewGuid(),
                OwnerId = owner2Id,
                Title = "Basic Room Timisoara",
                Description = "Simple room for students",
                City = "Timisoara",
                Area = "Student Area",
                Price = 250m,
                AvailableFrom = DateTime.UtcNow.AddDays(7),
                Amenities = "WiFi",
                ImagePaths = "",
                IsActive = false, // Inactive listing for testing
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            }
        };

        await _dbContext.Profiles.AddRangeAsync(owners);
        await _dbContext.RoomListings.AddRangeAsync(listings);
        await _dbContext.SaveChangesAsync();

        return owner1Id;
    }

    private async Task SeedLargeTestDataAsync()
    {
        var owner = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "bigowner@example.com",
            PasswordHash = "hash",
            FullName = "Big Owner",
            Bio = "Owner with many properties",
            Age = 50,
            Gender = "Male",
            University = "Business School",
            Lifestyle = "Professional",
            Interests = "Real Estate Empire",
            Role = "User",
            CreatedAt = DateTime.UtcNow.AddDays(-100)
        };

        await _dbContext.Profiles.AddAsync(owner);

        var listings = new List<RoomListing>();
        for (int i = 1; i <= 15; i++)
        {
            listings.Add(new RoomListing
            {
                Id = Guid.NewGuid(),
                OwnerId = owner.Id,
                Title = $"Property {i}",
                Description = $"Description for property {i}",
                City = "TestCity",
                Area = $"Area {i}",
                Price = 300m + (i * 50m),
                AvailableFrom = DateTime.UtcNow.AddDays(i),
                Amenities = "WiFi",
                ImagePaths = "",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }

        await _dbContext.RoomListings.AddRangeAsync(listings);
        await _dbContext.SaveChangesAsync();
    }
}
