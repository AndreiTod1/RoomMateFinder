using FluentAssertions;
using RoomMate_Finder.Common;
using RoomMate_Finder.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RoomMate_Finder.Test.Services;

public class JwtServiceTests
{
    private const string ValidKey = "very_long_secret_key_that_is_at_least_32_bytes_long_123456789";
    private const string Issuer = "test-issuer";
    private const string Audience = "test-audience";

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenKeyIsNull()
    {
        // Act
        Action act = () => _ = new JwtService(null!, Issuer, Audience);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*JWT key is not set*");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenKeyIsTooShort()
    {
        // Act
        Action act = () => _ = new JwtService("short", Issuer, Audience);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*JWT key is too short*");
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidTokenString()
    {
        // Arrange
        var service = new JwtService(ValidKey, Issuer, Audience);
        var user = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "Test User",
            Role = "User"
        };

        // Act
        var token = service.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        token.Split('.').Should().HaveCount(3); // Header.Payload.Signature
    }

    [Fact]
    public void GenerateToken_ShouldIncludeCorrectClaims()
    {
        // Arrange
        var service = new JwtService(ValidKey, Issuer, Audience);
        var userId = Guid.NewGuid();
        var user = new Profile
        {
            Id = userId,
            Email = "test@example.com",
            FullName = "Test User",
            Role = "Admin"
        };

        // Act
        var token = service.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be(Issuer);
        jwtToken.Audiences.Should().Contain(Audience);

        // Note: ReadJwtToken performs inbound mapping by default (nameid -> ClaimTypes.NameIdentifier)
        
        var claims = jwtToken.Claims.ToList();
        
        // Check ID (ClaimTypes.NameIdentifier)
        claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString());
        
        // Check Email
        claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        
        // Check Name
        claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == user.FullName);
        
        // Check Role
        claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GenerateToken_ShouldHaveCorrectExpiration()
    {
        // Arrange
        var service = new JwtService(ValidKey, Issuer, Audience);
        var user = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "Test User",
            Role = "User"
        };

        // Act
        var token = service.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        // Token should expire in about 7 days (with some tolerance)
        jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenKeyIsEmpty()
    {
        // Act
        Action act = () => _ = new JwtService("", Issuer, Audience);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*JWT key is not set*");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenKeyIsWhitespace()
    {
        // Act
        Action act = () => _ = new JwtService("   ", Issuer, Audience);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*JWT key is not set*");
    }

    [Fact]
    public void GenerateToken_ShouldGenerateDifferentTokensForDifferentUsers()
    {
        // Arrange
        var service = new JwtService(ValidKey, Issuer, Audience);
        var user1 = new Profile { Id = Guid.NewGuid(), Email = "user1@test.com", FullName = "User One", Role = "User" };
        var user2 = new Profile { Id = Guid.NewGuid(), Email = "user2@test.com", FullName = "User Two", Role = "Admin" };

        // Act
        var token1 = service.GenerateToken(user1);
        var token2 = service.GenerateToken(user2);

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void Constructor_ShouldAcceptKeyExactly16Bytes()
    {
        // Arrange - exactly 16 ASCII characters = 16 bytes
        var exactKey = "1234567890123456";

        // Act
        Action act = () => _ = new JwtService(exactKey, Issuer, Audience);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void GenerateToken_TokenShouldBeReadable()
    {
        // Arrange
        var service = new JwtService(ValidKey, Issuer, Audience);
        var user = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "Test User",
            Role = "User"
        };

        // Act
        var token = service.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Expiration should be roughly 7 days from now
        var expiration = jwtToken.ValidTo;
        var expectedExpiration = DateTime.UtcNow.AddDays(7);
        
        // Allow 1 minute variance
        expiration.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(1));
    }
}
