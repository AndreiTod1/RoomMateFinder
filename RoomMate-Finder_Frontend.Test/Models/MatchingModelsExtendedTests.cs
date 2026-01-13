using FluentAssertions;
using RoomMate_Finder_Frontend.Models;

namespace RoomMate_Finder_Frontend.Test.Models;

public class MatchingModelsExtendedTests
{
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
            FullName: "John Match",
            Age: 24,
            Gender: "Male",
            University: "Test University",
            Bio: "Looking for a great roommate",
            Lifestyle: "social",
            Interests: "Gaming, Music, Sports",
            MatchedAt: matchedAt,
            IsActive: true,
            ProfilePicturePath: "/images/john.jpg"
        );

        // Assert
        dto.MatchId.Should().Be(matchId);
        dto.UserId.Should().Be(userId);
        dto.Email.Should().Be("match@example.com");
        dto.FullName.Should().Be("John Match");
        dto.Age.Should().Be(24);
        dto.Gender.Should().Be("Male");
        dto.University.Should().Be("Test University");
        dto.Bio.Should().Be("Looking for a great roommate");
        dto.Lifestyle.Should().Be("social");
        dto.Interests.Should().Be("Gaming, Music, Sports");
        dto.MatchedAt.Should().Be(matchedAt);
        dto.IsActive.Should().BeTrue();
        dto.ProfilePicturePath.Should().Be("/images/john.jpg");
    }

    [Fact]
    public void UserMatchDto_ProfilePicturePath_ShouldBeNullByDefault()
    {
        // Act
        var dto = new UserMatchDto(
            Guid.NewGuid(), Guid.NewGuid(), "test@test.com", "Test", 25, "M",
            "Uni", "Bio", "quiet", "Music", DateTime.UtcNow, true
        );

        // Assert
        dto.ProfilePicturePath.Should().BeNull();
    }

    [Fact]
    public void UserMatchDto_IsActive_ShouldDefaultToTrue()
    {
        // Act
        var dto = new UserMatchDto(
            Guid.NewGuid(), Guid.NewGuid(), "test@test.com", "Test", 25, "M",
            "Uni", "Bio", "quiet", "Music", DateTime.UtcNow, true
        );

        // Assert
        dto.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UserMatchDto_Should_SupportInactiveMatches()
    {
        // Act
        var dto = new UserMatchDto(
            Guid.NewGuid(), Guid.NewGuid(), "test@test.com", "Test", 25, "M",
            "Uni", "Bio", "quiet", "Music", DateTime.UtcNow, false
        );

        // Assert
        dto.IsActive.Should().BeFalse();
    }

    [Fact]
    public void UserMatchDto_Should_SupportRecordEquality()
    {
        // Arrange
        var matchId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var matchedAt = DateTime.UtcNow;

        var dto1 = new UserMatchDto(matchId, userId, "test@test.com", "Test", 25, "M", "Uni", "Bio", "quiet", "Music", matchedAt, true);
        var dto2 = new UserMatchDto(matchId, userId, "test@test.com", "Test", 25, "M", "Uni", "Bio", "quiet", "Music", matchedAt, true);

        // Assert
        dto1.Should().Be(dto2);
    }

    #endregion

    #region LikeResponseDto Tests

    [Fact]
    public void LikeResponseDto_Should_HaveCorrectProperties_WhenSuccessful()
    {
        // Act
        var response = new LikeResponseDto(true, "Profile liked successfully");

        // Assert
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Profile liked successfully");
        response.IsMatch.Should().BeFalse();
        response.MatchId.Should().BeNull();
    }

    [Fact]
    public void LikeResponseDto_Should_IndicateMatch_WhenMatchOccurs()
    {
        // Arrange
        var matchId = Guid.NewGuid();

        // Act
        var response = new LikeResponseDto(true, "It's a match!", true, matchId);

        // Assert
        response.Success.Should().BeTrue();
        response.Message.Should().Be("It's a match!");
        response.IsMatch.Should().BeTrue();
        response.MatchId.Should().Be(matchId);
    }

    [Fact]
    public void LikeResponseDto_Should_HandleFailure()
    {
        // Act
        var response = new LikeResponseDto(false, "Could not like profile");

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Could not like profile");
        response.IsMatch.Should().BeFalse();
        response.MatchId.Should().BeNull();
    }

    [Fact]
    public void LikeResponseDto_IsMatch_ShouldDefaultToFalse()
    {
        // Act
        var response = new LikeResponseDto(true, "OK");

        // Assert
        response.IsMatch.Should().BeFalse();
    }

    [Fact]
    public void LikeResponseDto_MatchId_ShouldDefaultToNull()
    {
        // Act
        var response = new LikeResponseDto(true, "OK");

        // Assert
        response.MatchId.Should().BeNull();
    }

    #endregion

    #region PassResponseDto Tests

    [Fact]
    public void PassResponseDto_Should_HaveCorrectProperties_WhenSuccessful()
    {
        // Act
        var response = new PassResponseDto(true, "Profile passed successfully");

        // Assert
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Profile passed successfully");
    }

    [Fact]
    public void PassResponseDto_Should_HandleFailure()
    {
        // Act
        var response = new PassResponseDto(false, "Could not pass profile");

        // Assert
        response.Success.Should().BeFalse();
        response.Message.Should().Be("Could not pass profile");
    }

    [Fact]
    public void PassResponseDto_Should_SupportRecordEquality()
    {
        // Arrange
        var response1 = new PassResponseDto(true, "OK");
        var response2 = new PassResponseDto(true, "OK");

        // Assert
        response1.Should().Be(response2);
    }

    #endregion

    #region CompatibilityDto Tests

    [Fact]
    public void CompatibilityDto_Should_HaveCorrectProperties()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var details = new CompatibilityDetailsDto(
            85.0, 100.0, 90.0, 75.0, 80.0,
            "Similar age", "Same gender", "Same university", "Compatible lifestyle", "Shared interests"
        );

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
        dto.Details.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0, "Low")]
    [InlineData(50, "Medium")]
    [InlineData(75, "Good")]
    [InlineData(90, "Excellent")]
    [InlineData(100, "Perfect")]
    public void CompatibilityDto_Should_SupportVariousScoresAndLevels(double score, string level)
    {
        // Arrange
        var details = new CompatibilityDetailsDto(80, 80, 80, 80, 80, "", "", "", "", "");

        // Act
        var dto = new CompatibilityDto(
            Guid.NewGuid(), Guid.NewGuid(), score, level, details
        );

        // Assert
        dto.CompatibilityScore.Should().Be(score);
        dto.CompatibilityLevel.Should().Be(level);
    }

    #endregion

    #region CompatibilityDetailsDto Tests

    [Fact]
    public void CompatibilityDetailsDto_Should_HaveCorrectProperties()
    {
        // Act
        var details = new CompatibilityDetailsDto(
            AgeScore: 85.0,
            GenderScore: 100.0,
            UniversityScore: 90.0,
            LifestyleScore: 70.0,
            InterestsScore: 80.0,
            AgeDescription: "Within 2 years",
            GenderDescription: "Same gender",
            UniversityDescription: "Same university",
            LifestyleDescription: "Similar lifestyle",
            InterestsDescription: "3 shared interests"
        );

        // Assert
        details.AgeScore.Should().Be(85.0);
        details.GenderScore.Should().Be(100.0);
        details.UniversityScore.Should().Be(90.0);
        details.LifestyleScore.Should().Be(70.0);
        details.InterestsScore.Should().Be(80.0);
        details.AgeDescription.Should().Be("Within 2 years");
        details.GenderDescription.Should().Be("Same gender");
        details.UniversityDescription.Should().Be("Same university");
        details.LifestyleDescription.Should().Be("Similar lifestyle");
        details.InterestsDescription.Should().Be("3 shared interests");
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(50.0)]
    [InlineData(100.0)]
    public void CompatibilityDetailsDto_Scores_ShouldBeInValidRange(double score)
    {
        // Act
        var details = new CompatibilityDetailsDto(score, score, score, score, score, "", "", "", "", "");

        // Assert
        details.AgeScore.Should().BeInRange(0, 100);
        details.GenderScore.Should().BeInRange(0, 100);
        details.UniversityScore.Should().BeInRange(0, 100);
        details.LifestyleScore.Should().BeInRange(0, 100);
        details.InterestsScore.Should().BeInRange(0, 100);
    }

    [Fact]
    public void CompatibilityDetailsDto_Should_SupportRecordEquality()
    {
        // Arrange
        var details1 = new CompatibilityDetailsDto(80, 90, 70, 85, 75, "A", "B", "C", "D", "E");
        var details2 = new CompatibilityDetailsDto(80, 90, 70, 85, 75, "A", "B", "C", "D", "E");

        // Assert
        details1.Should().Be(details2);
    }

    #endregion
}

