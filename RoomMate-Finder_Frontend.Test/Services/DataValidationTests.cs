using FluentAssertions;
using RoomMate_Finder_Frontend.Models;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Services;

public class DataValidationTests
{
    #region Email Validation Tests

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user.name@domain.co.uk", true)]
    [InlineData("user+tag@example.com", true)]
    [InlineData("invalid-email", false)]
    [InlineData("@nodomain.com", false)]
    [InlineData("noat.domain.com", false)]
    [InlineData("", false)]
    public void Email_Validation_ShouldWork(string email, bool expectedValid)
    {
        // Act
        var isValid = IsValidEmail(email);

        // Assert
        isValid.Should().Be(expectedValid);
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Age Validation Tests

    [Theory]
    [InlineData(18, true)]
    [InlineData(25, true)]
    [InlineData(65, true)]
    [InlineData(17, false)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(150, false)]
    public void Age_Validation_ShouldWork(int age, bool expectedValid)
    {
        // Act
        var isValid = age >= 18 && age <= 100;

        // Assert
        isValid.Should().Be(expectedValid);
    }

    #endregion

    #region Price Validation Tests

    [Theory]
    [InlineData(100, true)]
    [InlineData(500, true)]
    [InlineData(0, false)]
    [InlineData(-100, false)]
    [InlineData(0.01, true)]
    [InlineData(10000, true)]
    public void Price_Validation_ShouldWork(decimal price, bool expectedValid)
    {
        // Act
        var isValid = price > 0;

        // Assert
        isValid.Should().Be(expectedValid);
    }

    #endregion

    #region Rating Validation Tests

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(4, true)]
    [InlineData(5, true)]
    [InlineData(0, false)]
    [InlineData(6, false)]
    [InlineData(-1, false)]
    public void Rating_Validation_ShouldWork(int rating, bool expectedValid)
    {
        // Act
        var isValid = rating >= 1 && rating <= 5;

        // Assert
        isValid.Should().Be(expectedValid);
    }

    #endregion

    #region Compatibility Score Validation Tests

    [Theory]
    [InlineData(0, true)]
    [InlineData(50, true)]
    [InlineData(100, true)]
    [InlineData(-1, false)]
    [InlineData(101, false)]
    public void CompatibilityScore_Validation_ShouldWork(double score, bool expectedValid)
    {
        // Act
        var isValid = score >= 0 && score <= 100;

        // Assert
        isValid.Should().Be(expectedValid);
    }

    #endregion

    #region String Length Validation Tests

    [Theory]
    [InlineData("Valid Name", 2, 50, true)]
    [InlineData("A", 2, 50, false)]
    [InlineData("", 2, 50, false)]
    [InlineData("This is a very long name that exceeds the maximum allowed length for testing purposes", 2, 50, false)]
    public void Name_Length_Validation_ShouldWork(string name, int minLength, int maxLength, bool expectedValid)
    {
        // Act
        var isValid = !string.IsNullOrEmpty(name) && name.Length >= minLength && name.Length <= maxLength;

        // Assert
        isValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData("Short bio", 500, true)]
    [InlineData("", 500, true)]
    public void Bio_Length_Validation_ShouldWork(string bio, int maxLength, bool expectedValid)
    {
        // Act
        var isValid = bio == null || bio.Length <= maxLength;

        // Assert
        isValid.Should().Be(expectedValid);
    }

    #endregion

    #region Date Validation Tests

    [Fact]
    public void AvailableFrom_ShouldNotBeInPast()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var futureDate = DateTime.UtcNow.AddDays(7);
        var today = DateTime.UtcNow.Date;

        // Assert
        (pastDate.Date >= today).Should().BeFalse();
        (futureDate.Date >= today).Should().BeTrue();
    }

    [Fact]
    public void CreatedAt_ShouldNotBeFuture()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);
        var pastDate = DateTime.UtcNow.AddDays(-1);

        // Assert
        (futureDate <= DateTime.UtcNow).Should().BeFalse();
        (pastDate <= DateTime.UtcNow).Should().BeTrue();
    }

    #endregion

    #region Guid Validation Tests

    [Fact]
    public void Guid_ShouldNotBeEmpty()
    {
        // Arrange
        var validGuid = Guid.NewGuid();
        var emptyGuid = Guid.Empty;

        // Assert
        (validGuid != Guid.Empty).Should().BeTrue();
        (emptyGuid != Guid.Empty).Should().BeFalse();
    }

    #endregion

    #region Lifestyle Values Tests

    [Theory]
    [InlineData("studious", true)]
    [InlineData("social", true)]
    [InlineData("active", true)]
    [InlineData("quiet", true)]
    [InlineData("balanced", true)]
    [InlineData("invalid", false)]
    [InlineData("", false)]
    public void Lifestyle_Values_ShouldBeValid(string lifestyle, bool expectedValid)
    {
        // Arrange
        var validLifestyles = new[] { "studious", "social", "active", "quiet", "balanced" };

        // Act
        var isValid = validLifestyles.Contains(lifestyle);

        // Assert
        isValid.Should().Be(expectedValid);
    }

    #endregion

    #region Gender Values Tests

    [Theory]
    [InlineData("Male", true)]
    [InlineData("Female", true)]
    [InlineData("Other", true)]
    [InlineData("Prefer not to say", true)]
    [InlineData("Invalid", false)]
    public void Gender_Values_ShouldBeValid(string gender, bool expectedValid)
    {
        // Arrange
        var validGenders = new[] { "Male", "Female", "Other", "Prefer not to say" };

        // Act
        var isValid = validGenders.Contains(gender);

        // Assert
        isValid.Should().Be(expectedValid);
    }

    #endregion
}

