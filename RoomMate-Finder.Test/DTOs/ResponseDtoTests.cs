using FluentAssertions;
using RoomMate_Finder.Features.Admins.GetAdmins;
using RoomMate_Finder.Features.Admins.GetAllUsers;
using RoomMate_Finder.Features.Matching.GetMatches;
using Xunit;

namespace RoomMate_Finder.Test.DTOs;

public class ResponseDtoTests
{
    #region ProfileResponse Tests

    [Fact]
    public void ProfileResponse_When_Created_Then_AllPropertiesAccessible()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var response = new ProfileResponse(
            Id: id,
            Email: "test@test.com",
            FullName: "John Doe",
            Age: 25,
            Gender: "Male",
            University: "University of Bucharest",
            Bio: "Software developer",
            Lifestyle: "Night Owl",
            Interests: "Gaming, Music",
            ProfilePicturePath: "/images/profile.jpg",
            CreatedAt: createdAt,
            Role: "User"
        );

        // Assert
        response.Id.Should().Be(id);
        response.Email.Should().Be("test@test.com");
        response.FullName.Should().Be("John Doe");
        response.Age.Should().Be(25);
        response.Gender.Should().Be("Male");
        response.University.Should().Be("University of Bucharest");
        response.Bio.Should().Be("Software developer");
        response.Lifestyle.Should().Be("Night Owl");
        response.Interests.Should().Be("Gaming, Music");
        response.ProfilePicturePath.Should().Be("/images/profile.jpg");
        response.CreatedAt.Should().Be(createdAt);
        response.Role.Should().Be("User");
    }

    [Fact]
    public void ProfileResponse_When_ProfilePicturePathNull_Then_ShouldBeValid()
    {
        // Arrange & Act
        var response = new ProfileResponse(
            Id: Guid.NewGuid(),
            Email: "test@test.com",
            FullName: "John Doe",
            Age: 25,
            Gender: "Male",
            University: "UB",
            Bio: "Bio",
            Lifestyle: "Night Owl",
            Interests: "Gaming",
            ProfilePicturePath: null,
            CreatedAt: DateTime.UtcNow,
            Role: "User"
        );

        // Assert
        response.ProfilePicturePath.Should().BeNull();
    }

    #endregion

    #region UserDto Tests

    [Fact]
    public void UserDto_When_Created_Then_AllPropertiesAccessible()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var dto = new UserDto(
            Id: id,
            Email: "admin@test.com",
            FullName: "Admin User",
            Age: 30,
            Gender: "Female",
            University: "ASE",
            ProfilePicturePath: "/admin.jpg",
            CreatedAt: createdAt,
            Role: "Admin"
        );

        // Assert
        dto.Id.Should().Be(id);
        dto.Email.Should().Be("admin@test.com");
        dto.FullName.Should().Be("Admin User");
        dto.Age.Should().Be(30);
        dto.Gender.Should().Be("Female");
        dto.University.Should().Be("ASE");
        dto.ProfilePicturePath.Should().Be("/admin.jpg");
        dto.CreatedAt.Should().Be(createdAt);
        dto.Role.Should().Be("Admin");
    }

    [Fact]
    public void UserDto_When_ProfilePicturePathNull_Then_ShouldBeValid()
    {
        // Arrange & Act
        var dto = new UserDto(
            Id: Guid.NewGuid(),
            Email: "test@test.com",
            FullName: "Test User",
            Age: 20,
            Gender: "Male",
            University: "UPB",
            ProfilePicturePath: null,
            CreatedAt: DateTime.UtcNow,
            Role: "User"
        );

        // Assert
        dto.ProfilePicturePath.Should().BeNull();
    }

    #endregion

    #region GetMatchesResponse Tests

    [Fact]
    public void GetMatchesResponse_When_Created_Then_AllPropertiesAccessible()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var response = new GetMatchesResponse(
            UserId: userId,
            Email: "match@test.com",
            FullName: "Match User",
            Age: 22,
            Gender: "Female",
            University: "University of Cluj",
            Bio: "Student and traveler",
            Lifestyle: "Early Bird",
            Interests: "Reading, Hiking, Photography",
            CompatibilityScore: 85.5,
            CompatibilityLevel: "High",
            CreatedAt: createdAt,
            ProfilePicturePath: "/match.jpg"
        );

        // Assert
        response.UserId.Should().Be(userId);
        response.Email.Should().Be("match@test.com");
        response.FullName.Should().Be("Match User");
        response.Age.Should().Be(22);
        response.Gender.Should().Be("Female");
        response.University.Should().Be("University of Cluj");
        response.Bio.Should().Be("Student and traveler");
        response.Lifestyle.Should().Be("Early Bird");
        response.Interests.Should().Be("Reading, Hiking, Photography");
        response.CompatibilityScore.Should().Be(85.5);
        response.CompatibilityLevel.Should().Be("High");
        response.CreatedAt.Should().Be(createdAt);
        response.ProfilePicturePath.Should().Be("/match.jpg");
    }

    [Fact]
    public void GetMatchesResponse_When_HighCompatibility_Then_ScoreAbove80()
    {
        // Arrange & Act
        var response = new GetMatchesResponse(
            UserId: Guid.NewGuid(),
            Email: "high@test.com",
            FullName: "High Match",
            Age: 25,
            Gender: "Male",
            University: "UB",
            Bio: "Bio",
            Lifestyle: "Night Owl",
            Interests: "Gaming",
            CompatibilityScore: 95.0,
            CompatibilityLevel: "Very High",
            CreatedAt: DateTime.UtcNow,
            ProfilePicturePath: null
        );

        // Assert
        response.CompatibilityScore.Should().BeGreaterThanOrEqualTo(80);
        response.CompatibilityLevel.Should().Contain("High");
    }

    [Fact]
    public void GetMatchesResponse_When_LowCompatibility_Then_ScoreBelow50()
    {
        // Arrange & Act
        var response = new GetMatchesResponse(
            UserId: Guid.NewGuid(),
            Email: "low@test.com",
            FullName: "Low Match",
            Age: 30,
            Gender: "Female",
            University: "ASE",
            Bio: "Different",
            Lifestyle: "Early Bird",
            Interests: "Sports",
            CompatibilityScore: 35.5,
            CompatibilityLevel: "Low",
            CreatedAt: DateTime.UtcNow,
            ProfilePicturePath: null
        );

        // Assert
        response.CompatibilityScore.Should().BeLessThan(50);
        response.CompatibilityLevel.Should().Be("Low");
    }

    [Fact]
    public void GetMatchesResponse_When_ProfilePicturePathNull_Then_ShouldBeValid()
    {
        // Arrange & Act
        var response = new GetMatchesResponse(
            UserId: Guid.NewGuid(),
            Email: "test@test.com",
            FullName: "Test",
            Age: 20,
            Gender: "Male",
            University: "UB",
            Bio: "",
            Lifestyle: "",
            Interests: "",
            CompatibilityScore: 50.0,
            CompatibilityLevel: "Medium",
            CreatedAt: DateTime.UtcNow,
            ProfilePicturePath: null
        );

        // Assert
        response.ProfilePicturePath.Should().BeNull();
    }

    #endregion
}
