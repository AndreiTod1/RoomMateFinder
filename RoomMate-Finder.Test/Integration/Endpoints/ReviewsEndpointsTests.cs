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
}
