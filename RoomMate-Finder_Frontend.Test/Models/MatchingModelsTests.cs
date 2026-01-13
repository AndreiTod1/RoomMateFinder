using FluentAssertions;
using RoomMate_Finder_Frontend.Models;

namespace RoomMate_Finder_Frontend.Test.Models;

public class MatchingModelsTests
{
    #region MatchProfileDto Tests

    [Fact]
    public void MatchProfileDto_Should_HaveCorrectProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var dto = new MatchProfileDto(
            UserId: userId,
            Email: "test@example.com",
            FullName: "Test User",
            Age: 25,
            Gender: "Male",
            University: "Test University",
            Bio: "Test Bio",
            Lifestyle: "Quiet",
            Interests: "Reading, Music",
            CompatibilityScore: 85.5,
            CompatibilityLevel: "High",
            CreatedAt: createdAt,
            ProfilePicturePath: "/images/test.jpg"
        );

        // Assert
        dto.UserId.Should().Be(userId);
        dto.Email.Should().Be("test@example.com");
        dto.FullName.Should().Be("Test User");
        dto.Age.Should().Be(25);
        dto.Gender.Should().Be("Male");
        dto.University.Should().Be("Test University");
        dto.Bio.Should().Be("Test Bio");
        dto.Lifestyle.Should().Be("Quiet");
        dto.Interests.Should().Be("Reading, Music");
        dto.CompatibilityScore.Should().Be(85.5);
        dto.CompatibilityLevel.Should().Be("High");
        dto.CreatedAt.Should().Be(createdAt);
        dto.ProfilePicturePath.Should().Be("/images/test.jpg");
    }

    [Fact]
    public void MatchProfileDto_ProfilePicturePath_ShouldBeNullByDefault()
    {
        // Act
        var dto = new MatchProfileDto(
            UserId: Guid.NewGuid(),
            Email: "test@example.com",
            FullName: "Test User",
            Age: 25,
            Gender: "Male",
            University: "Test University",
            Bio: "Bio",
            Lifestyle: "Quiet",
            Interests: "Music",
            CompatibilityScore: 75.0,
            CompatibilityLevel: "Good",
            CreatedAt: DateTime.UtcNow
        );

        // Assert
        dto.ProfilePicturePath.Should().BeNull();
    }

    [Fact]
    public void MatchProfileDto_Should_SupportRecordEquality()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var dto1 = new MatchProfileDto(userId, "test@test.com", "Test", 25, "M", "Uni", "Bio", "Quiet", "Music", 80, "High", createdAt);
        var dto2 = new MatchProfileDto(userId, "test@test.com", "Test", 25, "M", "Uni", "Bio", "Quiet", "Music", 80, "High", createdAt);

        // Assert
        dto1.Should().Be(dto2);
    }

    #endregion

    #region CompatibilityDetailsDto Tests

    [Fact]
    public void CompatibilityDetailsDto_Should_HaveCorrectProperties()
    {
        // Act
        var details = new CompatibilityDetailsDto(
            AgeScore: 90.0,
            GenderScore: 80.0,
            UniversityScore: 100.0,
            LifestyleScore: 75.0,
            InterestsScore: 85.0,
            AgeDescription: "Same age group",
            GenderDescription: "Same gender",
            UniversityDescription: "Same university",
            LifestyleDescription: "Compatible lifestyles",
            InterestsDescription: "Many common interests"
        );

        // Assert
        details.AgeScore.Should().Be(90.0);
        details.GenderScore.Should().Be(80.0);
        details.UniversityScore.Should().Be(100.0);
        details.LifestyleScore.Should().Be(75.0);
        details.InterestsScore.Should().Be(85.0);
        details.AgeDescription.Should().Be("Same age group");
        details.GenderDescription.Should().Be("Same gender");
        details.UniversityDescription.Should().Be("Same university");
        details.LifestyleDescription.Should().Be("Compatible lifestyles");
        details.InterestsDescription.Should().Be("Many common interests");
    }

    [Fact]
    public void CompatibilityDetailsDto_Should_CalculateAverageScore()
    {
        // Arrange
        var details = new CompatibilityDetailsDto(80, 70, 90, 60, 50, "", "", "", "", "");

        // Act
        var average = (details.AgeScore + details.GenderScore + details.UniversityScore + 
                      details.LifestyleScore + details.InterestsScore) / 5;

        // Assert
        average.Should().Be(70.0);
    }

    #endregion

    #region CompatibilityDto Tests

    [Fact]
    public void CompatibilityDto_Should_HaveCorrectProperties()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var details = new CompatibilityDetailsDto(90, 80, 100, 75, 85, "Age", "Gender", "Uni", "Life", "Int");

        // Act
        var dto = new CompatibilityDto(
            UserId1: userId1,
            UserId2: userId2,
            CompatibilityScore: 86.0,
            CompatibilityLevel: "High",
            Details: details
        );

        // Assert
        dto.UserId1.Should().Be(userId1);
        dto.UserId2.Should().Be(userId2);
        dto.CompatibilityScore.Should().Be(86.0);
        dto.CompatibilityLevel.Should().Be("High");
        dto.Details.Should().Be(details);
    }

    #endregion

    #region UserMatchDto Tests

    [Fact]
    public void UserMatchDto_Should_HaveCorrectProperties()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var matchedAt = DateTime.UtcNow;

        // Act
        var dto = new UserMatchDto(
            MatchId: matchId,
            UserId: userId,
            Email: "match@example.com",
            FullName: "Match User",
            Age: 28,
            Gender: "Female",
            University: "Other University",
            Bio: "Match Bio",
            Lifestyle: "Social",
            Interests: "Sports, Travel",
            MatchedAt: matchedAt,
            IsActive: true,
            ProfilePicturePath: "/images/match.jpg"
        );

        // Assert
        dto.MatchId.Should().Be(matchId);
        dto.UserId.Should().Be(userId);
        dto.Email.Should().Be("match@example.com");
        dto.FullName.Should().Be("Match User");
        dto.Age.Should().Be(28);
        dto.Gender.Should().Be("Female");
        dto.University.Should().Be("Other University");
        dto.Bio.Should().Be("Match Bio");
        dto.Lifestyle.Should().Be("Social");
        dto.Interests.Should().Be("Sports, Travel");
        dto.MatchedAt.Should().Be(matchedAt);
        dto.IsActive.Should().BeTrue();
        dto.ProfilePicturePath.Should().Be("/images/match.jpg");
    }

    [Fact]
    public void UserMatchDto_ProfilePicturePath_ShouldBeNullByDefault()
    {
        // Act
        var dto = new UserMatchDto(
            Guid.NewGuid(), Guid.NewGuid(), "test@test.com", "Test", 25, "M", "Uni", "Bio", "Quiet", "Music",
            DateTime.UtcNow, true
        );

        // Assert
        dto.ProfilePicturePath.Should().BeNull();
    }

    [Fact]
    public void UserMatchDto_IsActive_Should_BeSettable()
    {
        // Arrange & Act
        var activeDto = new UserMatchDto(
            Guid.NewGuid(), Guid.NewGuid(), "test@test.com", "Test", 25, "M", "Uni", "Bio", "Quiet", "Music",
            DateTime.UtcNow, true
        );
        var inactiveDto = new UserMatchDto(
            Guid.NewGuid(), Guid.NewGuid(), "test@test.com", "Test", 25, "M", "Uni", "Bio", "Quiet", "Music",
            DateTime.UtcNow, false
        );

        // Assert
        activeDto.IsActive.Should().BeTrue();
        inactiveDto.IsActive.Should().BeFalse();
    }

    #endregion

    #region LikeResponseDto Tests

    [Fact]
    public void LikeResponseDto_Should_HaveCorrectProperties_OnSuccess()
    {
        // Act
        var response = new LikeResponseDto(Success: true, Message: "Profile liked successfully");

        // Assert
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Profile liked successfully");
        response.IsMatch.Should().BeFalse();
        response.MatchId.Should().BeNull();
    }

    [Fact]
    public void LikeResponseDto_Should_IndicateMatch_WhenIsMatchTrue()
    {
        // Arrange
        var matchId = Guid.NewGuid();

        // Act
        var response = new LikeResponseDto(
            Success: true, 
            Message: "It's a match!", 
            IsMatch: true, 
            MatchId: matchId
        );

        // Assert
        response.Success.Should().BeTrue();
        response.IsMatch.Should().BeTrue();
        response.MatchId.Should().Be(matchId);
    }

    [Fact]
    public void LikeResponseDto_Should_HandleFailure()
    {
        // Act
        var response = new LikeResponseDto(Success: false, Message: "User not found");

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().Be("User not found");
    }

    #endregion

    #region PassResponseDto Tests

    [Fact]
    public void PassResponseDto_Should_HaveCorrectProperties_OnSuccess()
    {
        // Act
        var response = new PassResponseDto(Success: true, Message: "Profile passed");

        // Assert
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Profile passed");
    }

    [Fact]
    public void PassResponseDto_Should_HandleFailure()
    {
        // Act
        var response = new PassResponseDto(Success: false, Message: "Cannot pass yourself");

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Cannot pass yourself");
    }

    #endregion
}

