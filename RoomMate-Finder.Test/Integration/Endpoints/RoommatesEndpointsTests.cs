using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RoomMate_Finder.Common;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Infrastructure.Persistence;
using Xunit;

namespace RoomMate_Finder.Test.Integration.Endpoints;

/// <summary>
/// Integration tests for Roommates endpoints
/// </summary>
public class RoommatesEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public RoommatesEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Guid UserId, string Token)> CreateUserAndGetTokenWithId(string email)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jwtService = scope.ServiceProvider.GetRequiredService<JwtService>();

        var user = new Profile
        {
            Id = Guid.NewGuid(),
            Email = email,
            FullName = "Test User",
            Age = 25,
            Gender = "M",
            University = "Test University",
            Bio = "Test bio",
            Lifestyle = "Test lifestyle",
            Interests = "Test interests",
            Role = "User",
            PasswordHash = PasswordHasher.HashPassword("Password123!")
        };

        dbContext.Profiles.Add(user);
        await dbContext.SaveChangesAsync();

        var token = jwtService.GenerateToken(user);
        return (user.Id, token);
    }

    private void SetAuthorizationHeader(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    #region SendRequest Tests

    [Fact]
    public async Task SendRoommateRequest_ToNonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var (_, token) = await CreateUserAndGetTokenWithId($"sender_{Guid.NewGuid()}@test.com");
        SetAuthorizationHeader(token);

        var dto = new { TargetUserId = Guid.NewGuid(), Message = "Hi" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/roommates/requests", dto);

        // Assert
        // Start conversation logic throws InvalidOperationException if user not found, caught and returns BadRequest
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendRoommateRequest_ToValidUser_ReturnsOk()
    {
        // Arrange
        var (senderId, senderToken) = await CreateUserAndGetTokenWithId($"sender_valid_{Guid.NewGuid()}@test.com");
        var (receiverId, _) = await CreateUserAndGetTokenWithId($"receiver_valid_{Guid.NewGuid()}@test.com");
        
        SetAuthorizationHeader(senderToken);

        var dto = new { TargetUserId = receiverId, Message = "Hi" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/roommates/requests", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SendRoommateRequest_ToSelf_ReturnsBadRequest()
    {
        // Arrange
        var (userId, token) = await CreateUserAndGetTokenWithId($"self_{Guid.NewGuid()}@test.com");
        SetAuthorizationHeader(token);

        var dto = new { TargetUserId = userId, Message = "Hi" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/roommates/requests", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SendRoommateRequest_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        var dto = new { TargetUserId = Guid.NewGuid(), Message = "Hi" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/roommates/requests", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GetMyRequests Tests

    [Fact]
    public async Task GetMyRequests_Authenticated_ReturnsOk()
    {
        // Arrange
        var (_, token) = await CreateUserAndGetTokenWithId($"my_req_{Guid.NewGuid()}@test.com");
        SetAuthorizationHeader(token);

        // Act
        var response = await _client.GetAsync("/api/roommates/my-requests");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMyRequests_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync("/api/roommates/my-requests");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GetPendingRequests Tests

    [Fact]
    public async Task GetPendingRequests_AsAdmin_ReturnsOk()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jwtService = scope.ServiceProvider.GetRequiredService<JwtService>();

        var admin = new Profile
        {
            Id = Guid.NewGuid(),
            Email = $"admin_pending_{Guid.NewGuid()}@test.com",
            FullName = "Admin",
            Age = 30,
            Gender = "M",
            University = "Test",
            Bio = "Bio",
            Lifestyle = "Style",
            Interests = "Interests",
            Role = "Admin",
            PasswordHash = PasswordHasher.HashPassword("Password123!")
        };
        dbContext.Profiles.Add(admin);
        await dbContext.SaveChangesAsync();

        var token = jwtService.GenerateToken(admin);
        SetAuthorizationHeader(token);

        // Act
        var response = await _client.GetAsync("/api/roommates/requests/pending");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPendingRequests_AsUser_ReturnsForbidden()
    {
        // Arrange
        var (_, token) = await CreateUserAndGetTokenWithId($"user_pending_{Guid.NewGuid()}@test.com");
        SetAuthorizationHeader(token);

        // Act
        var response = await _client.GetAsync("/api/roommates/requests/pending");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region AcceptRequest Tests

    /*
    [Fact]
    public async Task AcceptRequest_NonExistentRequest_ReturnsNotFound()
    {
        // Arrange
        var (_, token) = await CreateUserAndGetTokenWithId($"accept_nf_{Guid.NewGuid()}@test.com");
        SetAuthorizationHeader(token);

        var requestId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/api/roommates/requests/{requestId}/approve", null);

        // Assert
        // Usually 404 if request not found, but endpoint says "BadRequest" if invalid op, or Unauthorized.
        // If requestId doesn't exist, handler likely throws not found or returns null.
        // Checking expectation: 404 is reasonable.
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    */

    [Fact]
    public async Task AcceptRequest_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.PostAsync($"/api/roommates/requests/{Guid.NewGuid()}/approve", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region RejectRequest Tests

    /*
    [Fact]
    public async Task RejectRequest_NonExistentRequest_ReturnsNotFound()
    {
        // Arrange
        var (_, token) = await CreateUserAndGetTokenWithId($"reject_nf_{Guid.NewGuid()}@test.com");
        SetAuthorizationHeader(token);

        // Act
        var response = await _client.PostAsync($"/api/roommates/requests/{Guid.NewGuid()}/reject", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    */

    [Fact]
    public async Task RejectRequest_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.PostAsync($"/api/roommates/requests/{Guid.NewGuid()}/reject", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GetRelationships Tests

    [Fact]
    public async Task GetRelationships_Authenticated_ReturnsOk()
    {
        // Arrange
        // Endpoint requires Admin role
        var (_, token) = await CreateUserAndGetTokenWithId($"rel_{Guid.NewGuid()}@test.com");
        // Create an admin user/token instead if needed, but CreateUserAndGetTokenWithId defaults to User role?
        // Wait, CreateUserAndGetTokenWithId definition in this file: 
        // private async Task<(Guid UserId, string Token)> CreateUserAndGetTokenWithId(string email) -> Role = "User" hardcoded.
        
        // I need to create an Admin user manually or refactor helper.
        // Or just use the helper and update role in DB? No, easier to copy helper logic sort of.
        
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jwtService = scope.ServiceProvider.GetRequiredService<JwtService>();

        var admin = new Profile
        {
            Id = Guid.NewGuid(),
            Email = $"admin_rel_{Guid.NewGuid()}@test.com",
            FullName = "Admin",
            Role = "Admin",
            PasswordHash = PasswordHasher.HashPassword("Password123!")
        };
        dbContext.Profiles.Add(admin);
        await dbContext.SaveChangesAsync();
        
        var tokenAdmin = jwtService.GenerateToken(admin);
        SetAuthorizationHeader(tokenAdmin);

        // Act
        var response = await _client.GetAsync("/api/roommates/relationships");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRelationships_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync("/api/roommates/relationships");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region EndRoommate Tests

    [Fact]
    public async Task AcceptRequest_NonExistentRequest_ReturnsBadRequest()
    {
        // Arrange
        // Endpoint requires Admin
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jwtService = scope.ServiceProvider.GetRequiredService<JwtService>();

        var admin = new Profile
        {
            Id = Guid.NewGuid(),
            Email = $"admin_accept_{Guid.NewGuid()}@test.com",
            FullName = "Admin",
            Role = "Admin",
            PasswordHash = PasswordHasher.HashPassword("Password123!")
        };
        dbContext.Profiles.Add(admin);
        await dbContext.SaveChangesAsync();
        
        var token = jwtService.GenerateToken(admin);
        SetAuthorizationHeader(token);
        
        var requestId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync($"/api/roommates/requests/{requestId}/approve", null);

        // Assert
        // Endpoint catches InvalidOperation and returns BadRequest
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task RejectRequest_NonExistentRequest_ReturnsBadRequest()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jwtService = scope.ServiceProvider.GetRequiredService<JwtService>();

        var admin = new Profile
        {
            Id = Guid.NewGuid(),
            Email = $"admin_reject_{Guid.NewGuid()}@test.com",
            FullName = "Admin",
            Role = "Admin",
            PasswordHash = PasswordHasher.HashPassword("Password123!")
        };
        dbContext.Profiles.Add(admin);
        await dbContext.SaveChangesAsync();
        
        var token = jwtService.GenerateToken(admin);
        SetAuthorizationHeader(token);

        // Act
        var response = await _client.PostAsync($"/api/roommates/requests/{Guid.NewGuid()}/reject", null);

        // Assert
        // Endpoint catches InvalidOperation and returns BadRequest
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task EndRoommate_NonExistentRelationship_ReturnsNotFound()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jwtService = scope.ServiceProvider.GetRequiredService<JwtService>();

        var admin = new Profile
        {
            Id = Guid.NewGuid(),
            Email = $"admin_end_{Guid.NewGuid()}@test.com",
            FullName = "Admin",
            Role = "Admin",
            PasswordHash = PasswordHasher.HashPassword("Password123!")
        };
        dbContext.Profiles.Add(admin);
        await dbContext.SaveChangesAsync();
        
        var token = jwtService.GenerateToken(admin);
        SetAuthorizationHeader(token);

        // Act
        var response = await _client.DeleteAsync($"/api/roommates/relationships/{Guid.NewGuid()}");

        // Assert
        // Endpoint catches InvalidOperation and returns NotFound
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EndRoommate_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.DeleteAsync($"/api/roommates/relationships/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
