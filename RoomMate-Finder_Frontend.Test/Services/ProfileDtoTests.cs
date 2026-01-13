using FluentAssertions;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Services;

public class ProfileDtoTests
{
    #region ProfileDto Tests

    [Fact]
    public void ProfileDto_Should_HaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var dto = new ProfileDto(
            Id: id,
            Email: "test@example.com",
            FullName: "Test User",
            Age: 25,
            Gender: "Male",
            University: "Test University",
            Bio: "Test Bio",
            Lifestyle: "Quiet",
            Interests: "Reading, Music",
            CreatedAt: createdAt,
            ProfilePicturePath: "/images/test.jpg",
            Role: "Admin"
        );

        // Assert
        dto.Id.Should().Be(id);
        dto.Email.Should().Be("test@example.com");
        dto.FullName.Should().Be("Test User");
        dto.Age.Should().Be(25);
        dto.Gender.Should().Be("Male");
        dto.University.Should().Be("Test University");
        dto.Bio.Should().Be("Test Bio");
        dto.Lifestyle.Should().Be("Quiet");
        dto.Interests.Should().Be("Reading, Music");
        dto.CreatedAt.Should().Be(createdAt);
        dto.ProfilePicturePath.Should().Be("/images/test.jpg");
        dto.Role.Should().Be("Admin");
    }

    [Fact]
    public void ProfileDto_Role_ShouldDefaultToUser()
    {
        // Act
        var dto = new ProfileDto(
            Id: Guid.NewGuid(),
            Email: "test@test.com",
            FullName: "Test",
            Age: 20,
            Gender: "F",
            University: "Uni",
            Bio: "Bio",
            Lifestyle: "Social",
            Interests: "Sports",
            CreatedAt: DateTime.UtcNow,
            ProfilePicturePath: null
        );

        // Assert
        dto.Role.Should().Be("User");
    }

    [Fact]
    public void ProfileDto_ProfilePicturePath_CanBeNull()
    {
        // Act
        var dto = new ProfileDto(
            Guid.NewGuid(), "test@test.com", "Test", 25, "M", "Uni", "Bio", "Quiet", "Music",
            DateTime.UtcNow, null, "User"
        );

        // Assert
        dto.ProfilePicturePath.Should().BeNull();
    }

    [Theory]
    [InlineData(18)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(100)]
    public void ProfileDto_Age_ShouldAcceptValidValues(int age)
    {
        // Act
        var dto = new ProfileDto(
            Guid.NewGuid(), "test@test.com", "Test", age, "M", "Uni", "Bio", "Quiet", "Music",
            DateTime.UtcNow, null, "User"
        );

        // Assert
        dto.Age.Should().Be(age);
    }

    [Fact]
    public void ProfileDto_Should_SupportRecordEquality()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var dto1 = new ProfileDto(id, "test@test.com", "Test", 25, "M", "Uni", "Bio", "Quiet", "Music", createdAt, null, "User");
        var dto2 = new ProfileDto(id, "test@test.com", "Test", 25, "M", "Uni", "Bio", "Quiet", "Music", createdAt, null, "User");

        // Assert
        dto1.Should().Be(dto2);
    }

    #endregion

    #region UpdateProfileRequestDto Tests

    [Fact]
    public void UpdateProfileRequestDto_Should_AllowAllNullValues()
    {
        // Act
        var dto = new UpdateProfileRequestDto(null, null, null, null, null, null, null);

        // Assert
        dto.FullName.Should().BeNull();
        dto.Age.Should().BeNull();
        dto.Gender.Should().BeNull();
        dto.University.Should().BeNull();
        dto.Bio.Should().BeNull();
        dto.Lifestyle.Should().BeNull();
        dto.Interests.Should().BeNull();
    }

    [Fact]
    public void UpdateProfileRequestDto_Should_AllowPartialUpdate()
    {
        // Act
        var dto = new UpdateProfileRequestDto(
            FullName: "New Name",
            Age: null,
            Gender: null,
            University: "New University",
            Bio: null,
            Lifestyle: null,
            Interests: null
        );

        // Assert
        dto.FullName.Should().Be("New Name");
        dto.Age.Should().BeNull();
        dto.University.Should().Be("New University");
    }

    [Fact]
    public void UpdateProfileRequestDto_Should_AllowFullUpdate()
    {
        // Act
        var dto = new UpdateProfileRequestDto(
            FullName: "Updated Name",
            Age: 30,
            Gender: "Female",
            University: "Updated University",
            Bio: "Updated Bio",
            Lifestyle: "Social",
            Interests: "Gaming, Movies"
        );

        // Assert
        dto.FullName.Should().Be("Updated Name");
        dto.Age.Should().Be(30);
        dto.Gender.Should().Be("Female");
        dto.University.Should().Be("Updated University");
        dto.Bio.Should().Be("Updated Bio");
        dto.Lifestyle.Should().Be("Social");
        dto.Interests.Should().Be("Gaming, Movies");
    }

    #endregion

    #region PaginatedUsersResponse Tests

    [Fact]
    public void PaginatedUsersResponse_Should_HaveCorrectProperties()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new UserDto(Guid.NewGuid(), "user1@test.com", "User One", 25, "M", "Uni1", null, DateTime.UtcNow, "User"),
            new UserDto(Guid.NewGuid(), "user2@test.com", "User Two", 28, "F", "Uni2", "/img/2.jpg", DateTime.UtcNow, "Admin")
        };

        // Act
        var response = new PaginatedUsersResponse(
            Users: users,
            TotalCount: 50,
            Page: 2,
            PageSize: 10
        );

        // Assert
        response.Users.Should().HaveCount(2);
        response.TotalCount.Should().Be(50);
        response.Page.Should().Be(2);
        response.PageSize.Should().Be(10);
    }

    [Fact]
    public void PaginatedUsersResponse_Should_HandleEmptyUsersList()
    {
        // Act
        var response = new PaginatedUsersResponse(
            Users: new List<UserDto>(),
            TotalCount: 0,
            Page: 1,
            PageSize: 10
        );

        // Assert
        response.Users.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
    }

    [Fact]
    public void PaginatedUsersResponse_Should_CalculateTotalPages()
    {
        // Arrange
        var response = new PaginatedUsersResponse(
            Users: new List<UserDto>(),
            TotalCount: 55,
            Page: 1,
            PageSize: 10
        );

        // Act
        var totalPages = (int)Math.Ceiling((double)response.TotalCount / response.PageSize);

        // Assert
        totalPages.Should().Be(6);
    }

    #endregion

    #region UserDto Tests

    [Fact]
    public void UserDto_Should_HaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var dto = new UserDto(
            Id: id,
            Email: "user@example.com",
            FullName: "User Name",
            Age: 30,
            Gender: "Female",
            University: "Some University",
            ProfilePicturePath: "/images/user.jpg",
            CreatedAt: createdAt,
            Role: "Admin"
        );

        // Assert
        dto.Id.Should().Be(id);
        dto.Email.Should().Be("user@example.com");
        dto.FullName.Should().Be("User Name");
        dto.Age.Should().Be(30);
        dto.Gender.Should().Be("Female");
        dto.University.Should().Be("Some University");
        dto.ProfilePicturePath.Should().Be("/images/user.jpg");
        dto.CreatedAt.Should().Be(createdAt);
        dto.Role.Should().Be("Admin");
    }

    [Fact]
    public void UserDto_ProfilePicturePath_CanBeNull()
    {
        // Act
        var dto = new UserDto(
            Guid.NewGuid(), "test@test.com", "Test", 25, "M", "Uni", null, DateTime.UtcNow, "User"
        );

        // Assert
        dto.ProfilePicturePath.Should().BeNull();
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Admin")]
    public void UserDto_Role_ShouldAcceptValidRoles(string role)
    {
        // Act
        var dto = new UserDto(
            Guid.NewGuid(), "test@test.com", "Test", 25, "M", "Uni", null, DateTime.UtcNow, role
        );

        // Assert
        dto.Role.Should().Be(role);
    }

    #endregion
}

