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
        Action act = () => new JwtService(null!, Issuer, Audience);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*JWT key is not set*");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenKeyIsTooShort()
    {
        // Act
        Action act = () => new JwtService("short", Issuer, Audience);

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

        // Expiration should be roughly 7 days from now
        var expiration = jwtToken.ValidTo;
        var expectedExpiration = DateTime.UtcNow.AddDays(7);
        
        // Allow 1 minute variance
        expiration.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(1));
    }
}
