using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Common;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Profiles.Login;
using RoomMate_Finder.Test.Helpers;
using RoomMate_Finder.Validators;
using Xunit;

namespace RoomMate_Finder.Test.Handlers;

public class LoginHandlerTests
{
    private static JwtService CreateTestJwtService()
    {
        // Cheie suficient de lungă pentru HS256 (>= 256 bits / 32 bytes)
        const string key = "test_secret_key_for_jwt_signing_123456";
        const string issuer = "test-issuer";
        const string audience = "test-audience";
        return new JwtService(key, issuer, audience);
    }

    private static LoginValidator CreateValidator()
    {
        return new LoginValidator();
    }

    private static LoginRequest CreateValidRequest(string email = "test@example.com", string password = "Str0ng!Pass1!")
    {
        return new LoginRequest(email, password);
    }

    [Fact]
    public async Task Given_InvalidRequest_When_HandleIsCalled_Then_UnauthorizedAccessExceptionIsThrown()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var jwtService = CreateTestJwtService();
        var validator = CreateValidator();
        var handler = new LoginHandler(context, jwtService, validator);

        var invalidRequest = new LoginRequest("invalid-email", "");

        // Act
        Func<Task> act = () => handler.Handle(invalidRequest, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<UnauthorizedAccessException>();
        ex.Which.Message.Should().Contain("A valid email address is required.");
        ex.Which.Message.Should().Contain("Password is required.");
    }

    [Fact]
    public async Task Given_NonexistentUser_When_HandleIsCalled_Then_UnauthorizedAccessExceptionIsThrown()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var jwtService = CreateTestJwtService();
        var validator = CreateValidator();
        var handler = new LoginHandler(context, jwtService, validator);

        var request = CreateValidRequest("nonexistent@example.com", "SomePassword123!");

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<UnauthorizedAccessException>();
        ex.Which.Message.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task Given_WrongPassword_When_HandleIsCalled_Then_UnauthorizedAccessExceptionIsThrown()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();

        // Create test user
        var hashedPassword = PasswordHasher.HashPassword("CorrectPassword123!");
        var existingProfile = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = hashedPassword,
            FullName = "Test User",
            Age = 30,
            Gender = "M",
            University = "Test Uni",
            Bio = "Test bio",
            Lifestyle = "Active",
            Interests = "Reading",
            CreatedAt = DateTime.UtcNow
        };
        context.Profiles.Add(existingProfile);
        await context.SaveChangesAsync();

        var jwtService = CreateTestJwtService();
        var validator = CreateValidator();
        var handler = new LoginHandler(context, jwtService, validator);

        var request = CreateValidRequest("test@example.com", "WrongPassword123!");

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<UnauthorizedAccessException>();
        ex.Which.Message.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task Given_ValidCredentials_When_HandleIsCalled_Then_AuthResponseIsReturned()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();

        var password = "CorrectPassword123!";
        var hashedPassword = PasswordHasher.HashPassword(password);
        var existingProfile = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = hashedPassword,
            FullName = "Test User",
            Age = 30,
            Gender = "M",
            University = "Test Uni",
            Bio = "Test bio",
            Lifestyle = "Active",
            Interests = "Reading",
            CreatedAt = DateTime.UtcNow
        };
        context.Profiles.Add(existingProfile);
        await context.SaveChangesAsync();

        var jwtService = CreateTestJwtService();
        var validator = CreateValidator();
        var handler = new LoginHandler(context, jwtService, validator);

        var request = CreateValidRequest("test@example.com", password);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(existingProfile.Id);
        result.Email.Should().Be(existingProfile.Email);
        result.FullName.Should().Be(existingProfile.FullName);
        result.Token.Should().NotBeNullOrWhiteSpace();
    }

}
