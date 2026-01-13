using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RoomMate_Finder.Common;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.RoomListings.CreateListing;
using RoomMate_Finder.Features.RoomListings.GetListingById;
using RoomMate_Finder.Features.RoomListings.SearchListings;
using RoomMate_Finder.Infrastructure.Persistence;
using Xunit;

namespace RoomMate_Finder.Test.Integration.Endpoints;

public class RoomListingsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public RoomListingsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region Helpers

    private async Task<(Profile User, string Token)> CreateUserAndGetTokenAsync(string emailPrefix)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jwtService = scope.ServiceProvider.GetRequiredService<JwtService>();

        var user = new Profile
        {
            Id = Guid.NewGuid(),
            Email = $"{emailPrefix}_{Guid.NewGuid()}@example.com",
            PasswordHash = PasswordHasher.HashPassword("TestPass123!"),
            FullName = "Test User",
            Age = 25,
            Gender = "Male",
            University = "Test Uni",
            Bio = "Bio",
            Lifestyle = "Quiet",
            Interests = "Coding",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        db.Profiles.Add(user);
        await db.SaveChangesAsync();

        var token = jwtService.GenerateToken(user);
        return (user, token);
    }

    #endregion

    #region Create Listing Tests

    [Fact]
    public async Task CreateListing_WithValidData_ReturnsOk()
    {
        // Arrange
        var (user, token) = await CreateUserAndGetTokenAsync("create_listing");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Test Title"), "Title");
        content.Add(new StringContent("Test Description"), "Description");
        content.Add(new StringContent("Test City"), "City");
        content.Add(new StringContent("Test Area"), "Area");
        content.Add(new StringContent("500"), "Price");
        content.Add(new StringContent(DateTime.UtcNow.AddDays(1).ToString("O")), "AvailableFrom");
        content.Add(new StringContent("WiFi,Parking"), "Amenities");

        // Add dummy image
        var fileContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(fileContent, "Images", "test_image.jpg");

        // Act
        var response = await _client.PostAsync("/room-listings", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CreateListingResponse>();
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Title");
        result.City.Should().Be("Test City");
    }

    [Fact]
    public async Task CreateListing_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient(); // No auth header
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("Title"), "Title");

        // Act
        var response = await client.PostAsync("/room-listings", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Search Listings Tests

    [Fact]
    public async Task SearchListings_WithValidFilters_ReturnsResults()
    {
        // Arrange - seed listings
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var owner = new Profile
            {
                Id = Guid.NewGuid(),
                Email = $"owner_{Guid.NewGuid()}@test.com",
                PasswordHash = "hash",
                FullName = "Owner",
                Age = 30,
                Gender = "Other",
                University = "Uni",
                Bio = "Bio",
                Lifestyle = "Style",
                Interests = "None",
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };
            db.Profiles.Add(owner);

            var listing1 = new RoomListing
            {
                Id = Guid.NewGuid(),
                OwnerId = owner.Id,
                Title = "Cheap Room",
                Description = "Desc",
                City = "Cluj",
                Area = "Center",
                Price = 300,
                AvailableFrom = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                ApprovalStatus = ListingApprovalStatus.Approved
            };
            
            var listing2 = new RoomListing
            {
                Id = Guid.NewGuid(),
                OwnerId = owner.Id,
                Title = "Expensive Room",
                Description = "Desc",
                City = "Bucuresti",
                Area = "North",
                Price = 800,
                AvailableFrom = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                ApprovalStatus = ListingApprovalStatus.Approved
            };

            db.RoomListings.AddRange(listing1, listing2);
            await db.SaveChangesAsync();
        }

        var searchRequest = new SearchListingsRequest
        {
            City = "Cluj",
            MaxPrice = 500
        };

        // Act
        var response = await _client.PostAsJsonAsync("/room-listings/search", searchRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SearchListingsResponse>();
        result.Should().NotBeNull();
        result!.Listings.Should().ContainSingle();
        result.Listings.First().Title.Should().Be("Cheap Room");
    }

    #endregion

    #region Get Listing By Id Tests

    [Fact]
    public async Task GetById_ExistingId_ReturnsListing()
    {
        // Arrange
        Guid listingId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var owner = new Profile
            {
                Id = Guid.NewGuid(),
                Email = $"owner_get_{Guid.NewGuid()}@test.com",
                PasswordHash = "hash",
                FullName = "Owner",
                Age = 30,
                Gender = "Other",
                University = "Uni",
                Bio = "Bio",
                Lifestyle = "Style",
                Interests = "None",
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };
            db.Profiles.Add(owner);

            var listing = new RoomListing
            {
                Id = Guid.NewGuid(),
                OwnerId = owner.Id,
                Title = "Get By Id Listing",
                Description = "Description",
                City = "City",
                Area = "Area",
                Price = 400,
                AvailableFrom = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                ApprovalStatus = ListingApprovalStatus.Approved
            };
            db.RoomListings.Add(listing);
            await db.SaveChangesAsync();
            listingId = listing.Id;
        }

        // Act
        var response = await _client.GetAsync($"/room-listings/{listingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetListingByIdResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(listingId);
        result.Title.Should().Be("Get By Id Listing");
    }

    [Fact]
    public async Task GetById_NonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/room-listings/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
