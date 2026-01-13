using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RoomMate_Finder.Common;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Profiles;
using RoomMate_Finder.Features.Profiles.Login;
using RoomMate_Finder.Infrastructure.Persistence;
using Xunit;

namespace RoomMate_Finder.Test.Integration.Endpoints;

public class ProfilesEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ProfilesEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region Login Endpoint Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange - seed a user
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var testUser = new Profile
        {
            Id = Guid.NewGuid(),
            Email = $"login_valid_{Guid.NewGuid()}@example.com",
            PasswordHash = PasswordHasher.HashPassword("TestPassword123!"),
            FullName = "Login Test User",
            Age = 25,
            Gender = "Male",
            University = "Test University",
            Bio = "Test bio",
            Lifestyle = "Night Owl",
            Interests = "Testing",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
        db.Profiles.Add(testUser);
        await db.SaveChangesAsync();

        var loginRequest = new LoginRequest(testUser.Email, "TestPassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/profiles/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.Email.Should().Be(testUser.Email);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange - seed a user
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var testUser = new Profile
        {
            Id = Guid.NewGuid(),
            Email = $"login_invalid_{Guid.NewGuid()}@example.com",
            PasswordHash = PasswordHasher.HashPassword("CorrectPassword123!"),
            FullName = "Invalid Login User",
            Age = 25,
            Gender = "Male",
            University = "Test University",
            Bio = "Test bio",
            Lifestyle = "Night Owl",
            Interests = "Testing",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
        db.Profiles.Add(testUser);
        await db.SaveChangesAsync();

        var loginRequest = new LoginRequest(testUser.Email, "WrongPassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/profiles/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest($"nonexistent_{Guid.NewGuid()}@example.com", "AnyPassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/profiles/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region CreateProfile Endpoint Tests

    [Fact]
    public async Task CreateProfile_WithValidData_ReturnsOkWithToken()
    {
        // Arrange - use multipart/form-data
        var uniqueEmail = $"newuser_{Guid.NewGuid()}@example.com";
        
        using var formData = new MultipartFormDataContent();
        formData.Add(new StringContent(uniqueEmail), "Email");
        formData.Add(new StringContent("Password123!"), "Password");
        formData.Add(new StringContent("New Test User"), "FullName");
        formData.Add(new StringContent("25"), "Age");
        formData.Add(new StringContent("Male"), "Gender");
        formData.Add(new StringContent("Test University"), "University");
        formData.Add(new StringContent("Test Bio"), "Bio");
        formData.Add(new StringContent("Night Owl"), "Lifestyle");
        formData.Add(new StringContent("Testing, Coding"), "Interests");

        // Act
        var response = await _client.PostAsync("/profiles", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("token");
    }

    [Fact]
    public async Task CreateProfile_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange - first create a user
        var duplicateEmail = $"duplicate_{Guid.NewGuid()}@example.com";
        
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var existingUser = new Profile
        {
            Id = Guid.NewGuid(),
            Email = duplicateEmail,
            PasswordHash = PasswordHasher.HashPassword("Password123!"),
            FullName = "Existing User",
            Age = 25,
            Gender = "Male",
            University = "Test University",
            Bio = "Bio",
            Lifestyle = "Night Owl",
            Interests = "Testing",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
        db.Profiles.Add(existingUser);
        await db.SaveChangesAsync();
        
        // Now try to create another user with same email
        using var formData = new MultipartFormDataContent();
        formData.Add(new StringContent(duplicateEmail), "Email");
        formData.Add(new StringContent("Password123!"), "Password");
        formData.Add(new StringContent("Another User"), "FullName");
        formData.Add(new StringContent("30"), "Age");
        formData.Add(new StringContent("Female"), "Gender");
        formData.Add(new StringContent("Another University"), "University");
        formData.Add(new StringContent("Another Bio"), "Bio");
        formData.Add(new StringContent("Early Bird"), "Lifestyle");
        formData.Add(new StringContent("Reading"), "Interests");

        // Act
        var response = await _client.PostAsync("/profiles", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GetProfiles Endpoint Tests

    [Fact]
    public async Task GetProfiles_WithValidToken_ReturnsOk()
    {
        // Arrange - create user and get token
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jwtService = scope.ServiceProvider.GetRequiredService<JwtService>();
        
        var testUser = new Profile
        {
            Id = Guid.NewGuid(),
            Email = $"getprofiles_{Guid.NewGuid()}@example.com",
            PasswordHash = PasswordHasher.HashPassword("Password123!"),
            FullName = "Get Profiles User",
            Age = 25,
            Gender = "Male",
            University = "Test University",
            Bio = "Bio",
            Lifestyle = "Night Owl",
            Interests = "Testing",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
        db.Profiles.Add(testUser);
        await db.SaveChangesAsync();

        var token = jwtService.GenerateToken(testUser);
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/profiles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProfiles_WithoutToken_ReturnsOk()
    {
        // Note: GetProfiles endpoint does not require authorization
        // Arrange - new client without auth
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/profiles");

        // Assert - endpoint is publicly accessible
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
