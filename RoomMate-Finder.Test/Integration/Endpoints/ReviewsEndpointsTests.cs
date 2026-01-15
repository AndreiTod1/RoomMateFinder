using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RoomMate_Finder.Common;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Reviews.CreateReview;
using RoomMate_Finder.Features.Reviews.GetUserReviews;
using RoomMate_Finder.Features.Reviews.GetReviewStats; // Check namespace
using RoomMate_Finder.Infrastructure.Persistence;
using Xunit;

namespace RoomMate_Finder.Test.Integration.Endpoints;

public class ReviewsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ReviewsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Profile Reviewer, Profile Reviewee, string Token)> SeedUsersAsync(string prefix)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jwtService = scope.ServiceProvider.GetRequiredService<JwtService>();

        var reviewer = new Profile
        {
            Id = Guid.NewGuid(),
            Email = $"{prefix}_r_{Guid.NewGuid()}@example.com",
            PasswordHash = "hash",
            FullName = "Reviewer",
            Age = 20,
            Gender = "Male",
            University = "Uni",
            Bio = "Bio",
            Lifestyle = "Quiet",
            Interests = "Coding",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        var reviewee = new Profile
        {
            Id = Guid.NewGuid(),
            Email = $"{prefix}_e_{Guid.NewGuid()}@example.com",
            PasswordHash = "hash",
            FullName = "Reviewee",
            Age = 21,
            Gender = "Female",
            University = "Uni",
            Bio = "Bio",
            Lifestyle = "Quiet",
            Interests = "Reading",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        db.Profiles.AddRange(reviewer, reviewee);
        await db.SaveChangesAsync();

        var token = jwtService.GenerateToken(reviewer);
        return (reviewer, reviewee, token);
    }

    [Fact]
    public async Task CreateReview_ValidReview_ReturnsOk()
    {
        // Arrange
        var (reviewer, reviewee, token) = await SeedUsersAsync("create_review");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var request = new CreateReviewEndpoint.CreateReviewRequestBody(5, "Great roommate!");

        // Act
        var response = await _client.PostAsJsonAsync($"/profiles/{reviewee.Id}/reviews", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CreateReviewResponse>();
        result.Should().NotBeNull();
        result!.Rating.Should().Be(5);
        result.Comment.Should().Be("Great roommate!");
    }

    [Fact]
    public async Task CreateReview_SelfReview_ReturnsBadRequest()
    {
        // Arrange
        var (reviewer, reviewee, token) = await SeedUsersAsync("self_review");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var request = new CreateReviewEndpoint.CreateReviewRequestBody(5, "I am great!");

        // Act
        var response = await _client.PostAsJsonAsync($"/profiles/{reviewer.Id}/reviews", request);

        // Assert - expecting 400 Bad Request as self-reviews should be prevented
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    // Additional tests for GetUserReviews and Stats if endpoints are available/known
    // Assuming GetUserReviews is GET /profiles/{userId}/reviews
    // Assuming GetReviewStats is GET /profiles/{userId}/reviews/stats
    [Fact]
    public async Task GetUserReviews_ReturnsReviews()
    {
        // Arrange
        var (reviewer, reviewee, token) = await SeedUsersAsync("get_reviews");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        // Create a review first
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Reviews.Add(new Review
            {
                Id = Guid.NewGuid(),
                ReviewerId = reviewer.Id,
                ReviewedUserId = reviewee.Id,
                Rating = 4,
                Comment = "Good",
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync($"/profiles/{reviewee.Id}/reviews");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetUserReviewsResponse>();
        result.Should().NotBeNull();
        result!.Reviews.Should().ContainSingle();
        result.Reviews.First().Rating.Should().Be(4);
    }

    [Fact]
    public async Task GetReviewStats_ReturnsStats()
    {
        // Arrange
        var (reviewer, reviewee, token) = await SeedUsersAsync("get_stats");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        // Create review
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Reviews.Add(new Review
            {
                Id = Guid.NewGuid(),
                ReviewerId = reviewer.Id,
                ReviewedUserId = reviewee.Id,
                Rating = 5,
                Comment = "Excellent",
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync($"/profiles/{reviewee.Id}/reviews/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetReviewStatsResponse>();
        result.Should().NotBeNull();
        result!.AverageRating.Should().Be(5);
        result.TotalReviews.Should().Be(1);
    }
}
