using FluentAssertions;
using RoomMate_Finder.Common;
using Xunit;

namespace RoomMate_Finder.Test.Services;

public class PasswordHasherTests
{
    [Fact]
    public void Given_Password_When_HashPassword_Then_ReturnsNonEmptyHash()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = PasswordHasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Given_SamePassword_When_HashPasswordCalledTwice_Then_ReturnsSameHash()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = PasswordHasher.HashPassword(password);
        var hash2 = PasswordHasher.HashPassword(password);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Given_DifferentPasswords_When_HashPassword_Then_ReturnsDifferentHashes()
    {
        // Arrange
        var password1 = "TestPassword123!";
        var password2 = "DifferentPassword456!";

        // Act
        var hash1 = PasswordHasher.HashPassword(password1);
        var hash2 = PasswordHasher.HashPassword(password2);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Given_CorrectPassword_When_VerifyPassword_Then_ReturnsTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = PasswordHasher.HashPassword(password);

        // Act
        var result = PasswordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Given_IncorrectPassword_When_VerifyPassword_Then_ReturnsFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword!";
        var hash = PasswordHasher.HashPassword(password);

        // Act
        var result = PasswordHasher.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Given_EmptyPassword_When_HashPassword_Then_ReturnsHash()
    {
        // Arrange
        var password = "";

        // Act
        var hash = PasswordHasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Given_EmptyPassword_When_VerifyPassword_Then_WorksCorrectly()
    {
        // Arrange
        var password = "";
        var hash = PasswordHasher.HashPassword(password);

        // Act
        var result = PasswordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Given_SpecialCharactersPassword_When_HashAndVerify_Then_WorksCorrectly()
    {
        // Arrange
        var password = "P@$$w0rd!#$%^&*()_+-=[]{}|;':\",./<>?";

        // Act
        var hash = PasswordHasher.HashPassword(password);
        var result = PasswordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Given_UnicodePassword_When_HashAndVerify_Then_WorksCorrectly()
    {
        // Arrange
        var password = "パスワード密码كلمة السر";

        // Act
        var hash = PasswordHasher.HashPassword(password);
        var result = PasswordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Given_LongPassword_When_HashAndVerify_Then_WorksCorrectly()
    {
        // Arrange
        var password = new string('A', 1000);

        // Act
        var hash = PasswordHasher.HashPassword(password);
        var result = PasswordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Given_Password_When_Hash_Then_HashIsBase64Encoded()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = PasswordHasher.HashPassword(password);

        // Assert - verify it's valid base64
        Action act = () => Convert.FromBase64String(hash);
        act.Should().NotThrow();
    }

    [Fact]
    public void Given_CaseSensitivePasswords_When_HashPassword_Then_ReturnsDifferentHashes()
    {
        // Arrange
        var password1 = "TestPassword";
        var password2 = "testpassword";

        // Act
        var hash1 = PasswordHasher.HashPassword(password1);
        var hash2 = PasswordHasher.HashPassword(password2);

        // Assert
        hash1.Should().NotBe(hash2);
    }
}

