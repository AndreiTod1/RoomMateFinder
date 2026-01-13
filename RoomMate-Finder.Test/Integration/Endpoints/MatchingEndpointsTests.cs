using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RoomMate_Finder.Common;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Matching.GetMatches;
using RoomMate_Finder.Features.Matching.GetUserMatches;
using RoomMate_Finder.Features.Matching.LikeProfile;
using RoomMate_Finder.Infrastructure.Persistence;
using Xunit;

namespace RoomMate_Finder.Test.Integration.Endpoints;

public class MatchingEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public MatchingEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(); // Authorization not strictly enforced on these endpoints based on code inspection
    }

    private async Task<(Profile UserA, Profile UserB)> SeedTwoUsersAsync(string prefix)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var userA = new Profile
        {
            Id = Guid.NewGuid(),
            Email = $"{prefix}_a_{Guid.NewGuid()}@example.com",
            PasswordHash = "hash",
            FullName = "User A",
            Age = 20,
            Gender = "Male",
            University = "Uni",
            Bio = "Bio",
            Lifestyle = "Quiet",
            Interests = "Coding",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        var userB = new Profile
        {
            Id = Guid.NewGuid(),
            Email = $"{prefix}_b_{Guid.NewGuid()}@example.com",
            PasswordHash = "hash",
            FullName = "User B",
            Age = 21,
            Gender = "Female",
            University = "Uni",
            Bio = "Bio",
            Lifestyle = "Quiet",
            Interests = "Reading",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        db.Profiles.AddRange(userA, userB);
        await db.SaveChangesAsync();

        return (userA, userB);
    }

    [Fact]
    public async Task Discover_ReturnsPotentialMatches()
    {
        // Arrange
        var (userA, userB) = await SeedTwoUsersAsync("discover");

        // Act
        var response = await _client.GetAsync($"/matching/matches/{userA.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var matches = await response.Content.ReadFromJsonAsync<List<GetMatchesResponse>>();
        matches.Should().NotBeNull();
        matches.Should().Contain(m => m.UserId == userB.Id);
    }

    [Fact]
    public async Task LikeProfile_FirstLike_ReturnsSuccessNoMatch()
    {
        // Arrange
        var (userA, userB) = await SeedTwoUsersAsync("like_single");
        var request = new LikeProfileRequest(userA.Id, userB.Id);

        // Act
        var response = await _client.PostAsJsonAsync("/matching/like", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LikeProfileResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.IsMatch.Should().BeFalse();
    }

    [Fact]
    public async Task LikeProfile_MutualLike_ReturnsMatch()
    {
        // Arrange
        var (userA, userB) = await SeedTwoUsersAsync("like_mutual");

        // First like: A likes B
        await _client.PostAsJsonAsync("/matching/like", new LikeProfileRequest(userA.Id, userB.Id));

        // Act: B likes A
        var request = new LikeProfileRequest(userB.Id, userA.Id);
        var response = await _client.PostAsJsonAsync("/matching/like", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LikeProfileResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.IsMatch.Should().BeTrue();
        result.MatchId.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMyMatches_ReturnsMatchedProfiles()
    {
        // Arrange
        var (userA, userB) = await SeedTwoUsersAsync("get_my_matches");

        // Create mutual like (Match)
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            var like1 = new UserAction { UserId = userA.Id, TargetUserId = userB.Id, ActionType = ActionType.Like, CreatedAt = DateTime.UtcNow };
            var like2 = new UserAction { UserId = userB.Id, TargetUserId = userA.Id, ActionType = ActionType.Like, CreatedAt = DateTime.UtcNow };
            db.UserActions.AddRange(like1, like2);

            var match = new Match 
            { 
                Id = Guid.NewGuid(), 
                User1Id = userA.Id, 
                User2Id = userB.Id, 
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            db.Matches.Add(match);
            await db.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync($"/matching/my-matches/{userA.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var matches = await response.Content.ReadFromJsonAsync<List<GetUserMatchesResponse>>();
        matches.Should().NotBeNull();
        matches.Should().Contain(m => m.UserId == userB.Id);
    }
}
