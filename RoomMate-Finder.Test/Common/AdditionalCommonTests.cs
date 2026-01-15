using FluentAssertions;
using RoomMate_Finder.Common;
using RoomMate_Finder.Entities;

namespace RoomMate_Finder.Test.Common;

/// <summary>
/// Additional tests for common utility classes.
/// </summary>
public class AdditionalCommonTests
{
    #region PasswordHasher Additional Tests

    [Fact]
    public void Given_Password_When_Hashed_Then_ReturnsNonEmptyString()
    {
        // Arrange
        const string password = "TestPassword123!";

        // Act
        var hash = PasswordHasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Given_Password_When_HashedMultipleTimes_Then_AllVerifyCorrectly()
    {
        // Arrange
        const string password = "SamePassword123!";

        // Act - BCrypt generates different hashes due to random salt
        var hash1 = PasswordHasher.HashPassword(password);
        var hash2 = PasswordHasher.HashPassword(password);

        // Assert - Both hashes should verify against the original password
        PasswordHasher.VerifyPassword(password, hash1).Should().BeTrue();
        PasswordHasher.VerifyPassword(password, hash2).Should().BeTrue();
    }

    [Fact]
    public void Given_CorrectPassword_When_Verified_Then_ReturnsTrue()
    {
        // Arrange
        const string password = "CorrectPassword123!";
        var hash = PasswordHasher.HashPassword(password);

        // Act
        var result = PasswordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Given_WrongPassword_When_Verified_Then_ReturnsFalse()
    {
        // Arrange
        const string correctPassword = "CorrectPassword123!";
        const string wrongPassword = "WrongPassword456!";
        var hash = PasswordHasher.HashPassword(correctPassword);

        // Act
        var result = PasswordHasher.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Given_EmptyPassword_When_Hashed_Then_StillCreatesHash()
    {
        // Arrange
        const string password = "";

        // Act
        var hash = PasswordHasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Given_LongPassword_When_HashedAndVerified_Then_Works()
    {
        // Arrange
        var longPassword = new string('a', 100);

        // Act
        var hash = PasswordHasher.HashPassword(longPassword);
        var result = PasswordHasher.VerifyPassword(longPassword, hash);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        result.Should().BeTrue();
    }

    [Fact]
    public void Given_SpecialCharacters_When_HashedAndVerified_Then_Works()
    {
        // Arrange
        const string password = "P@$$w0rd!#$%^&*()_+-=[]{}|;':\",./<>?";

        // Act
        var hash = PasswordHasher.HashPassword(password);
        var result = PasswordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Given_UnicodeCharacters_When_HashedAndVerified_Then_Works()
    {
        // Arrange
        const string password = "Пароль密码パスワード";

        // Act
        var hash = PasswordHasher.HashPassword(password);
        var result = PasswordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("simple")]
    [InlineData("Complex1!")]
    [InlineData("with spaces")]
    [InlineData("12345678")]
    public void Given_VariousPasswords_When_HashedAndVerified_Then_AllWork(string password)
    {
        // Act
        var hash = PasswordHasher.HashPassword(password);
        var result = PasswordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Given_CaseSensitivePassword_When_WrongCase_Then_ReturnsFalse()
    {
        // Arrange
        const string password = "CaseSensitive";
        var hash = PasswordHasher.HashPassword(password);

        // Act
        var result = PasswordHasher.VerifyPassword("casesensitive", hash);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region JwtService Tests

    [Fact]
    public void Given_JwtService_When_Created_Then_NotNull()
    {
        // Arrange & Act
        var service = new JwtService(
            "test-secret-key-that-is-long-enough-for-hmac-sha256",
            "test-issuer",
            "test-audience"
        );

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Given_JwtService_When_GenerateToken_Then_ReturnsToken()
    {
        // Arrange
        var service = new JwtService(
            "test-secret-key-that-is-long-enough-for-hmac-sha256-algorithm",
            "test-issuer",
            "test-audience"
        );
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
        token.Should().NotBeNullOrEmpty();
        token.Should().Contain("."); // JWT format
    }

    [Fact]
    public void Given_JwtService_When_GenerateMultipleTokens_Then_AllDifferent()
    {
        // Arrange
        var service = new JwtService(
            "test-secret-key-that-is-long-enough-for-hmac-sha256-algorithm",
            "test-issuer",
            "test-audience"
        );
        var user1 = new Profile { Id = Guid.NewGuid(), Email = "user1@test.com", FullName = "User1", Role = "User" };
        var user2 = new Profile { Id = Guid.NewGuid(), Email = "user2@test.com", FullName = "User2", Role = "User" };

        // Act
        var token1 = service.GenerateToken(user1);
        var token2 = service.GenerateToken(user2);

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void Given_JwtService_When_AdminToken_Then_TokenGenerated()
    {
        // Arrange
        var service = new JwtService(
            "test-secret-key-that-is-long-enough-for-hmac-sha256-algorithm",
            "test-issuer",
            "test-audience"
        );
        var admin = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "admin@test.com",
            FullName = "Admin User",
            Role = "Admin"
        };

        // Act
        var token = service.GenerateToken(admin);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Admin")]
    [InlineData("Moderator")]
    public void Given_JwtService_When_DifferentRoles_Then_TokensGenerated(string role)
    {
        // Arrange
        var service = new JwtService(
            "test-secret-key-that-is-long-enough-for-hmac-sha256-algorithm",
            "test-issuer",
            "test-audience"
        );
        var user = new Profile
        {
            Id = Guid.NewGuid(),
            Email = $"{role.ToLower()}@test.com",
            FullName = $"{role} User",
            Role = role
        };

        // Act
        var token = service.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Given_JwtService_When_NullKey_Then_ThrowsArgumentException()
    {
        // Arrange & Act
        Action act = () => _ = new JwtService(null!, "issuer", "audience");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Given_JwtService_When_EmptyKey_Then_ThrowsArgumentException()
    {
        // Arrange & Act
        Action act = () => _ = new JwtService("", "issuer", "audience");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Given_JwtService_When_ShortKey_Then_ThrowsArgumentException()
    {
        // Arrange & Act (key too short - less than 16 bytes)
        Action act = () => _ = new JwtService("short", "issuer", "audience");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion
}
