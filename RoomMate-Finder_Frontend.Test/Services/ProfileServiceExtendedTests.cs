using FluentAssertions;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Services;

public class ProfileServiceExtendedTests
{
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
    public void UpdateProfileRequestDto_Should_AcceptPartialValues()
    {
        // Act
        var dto = new UpdateProfileRequestDto(
            FullName: "New Name",
            Age: 25,
            Gender: null,
            University: "New University",
            Bio: null,
            Lifestyle: null,
            Interests: null
        );

        // Assert
        dto.FullName.Should().Be("New Name");
        dto.Age.Should().Be(25);
        dto.Gender.Should().BeNull();
        dto.University.Should().Be("New University");
    }

    [Fact]
    public void UpdateProfileRequestDto_Should_AcceptAllValues()
    {
        // Act
        var dto = new UpdateProfileRequestDto(
            FullName: "Complete Name",
            Age: 30,
            Gender: "Female",
            University: "Top University",
            Bio: "This is my bio",
            Lifestyle: "active",
            Interests: "Sports, Music, Travel"
        );

        // Assert
        dto.FullName.Should().Be("Complete Name");
        dto.Age.Should().Be(30);
        dto.Gender.Should().Be("Female");
        dto.University.Should().Be("Top University");
        dto.Bio.Should().Be("This is my bio");
        dto.Lifestyle.Should().Be("active");
        dto.Interests.Should().Be("Sports, Music, Travel");
    }

    [Theory]
    [InlineData(18)]
    [InlineData(25)]
    [InlineData(35)]
    [InlineData(65)]
    public void UpdateProfileRequestDto_Should_AcceptValidAges(int age)
    {
        // Act
        var dto = new UpdateProfileRequestDto(null, age, null, null, null, null, null);

        // Assert
        dto.Age.Should().Be(age);
    }

    #endregion

    #region PaginatedUsersResponse Tests

    [Fact]
    public void PaginatedUsersResponse_Should_HaveCorrectProperties()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new UserDto(Guid.NewGuid(), "user1@test.com", "User One", 25, "Male", "University A", null, DateTime.UtcNow, "User"),
            new UserDto(Guid.NewGuid(), "user2@test.com", "User Two", 28, "Female", "University B", "/images/pic.jpg", DateTime.UtcNow, "Admin")
        };

        // Act
        var response = new PaginatedUsersResponse(users, 50, 1, 10);

        // Assert
        response.Users.Should().HaveCount(2);
        response.TotalCount.Should().Be(50);
        response.Page.Should().Be(1);
        response.PageSize.Should().Be(10);
    }

    [Fact]
    public void PaginatedUsersResponse_Should_HandleEmptyResults()
    {
        // Act
        var response = new PaginatedUsersResponse(new List<UserDto>(), 0, 1, 10);

        // Assert
        response.Users.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(2, 20)]
    [InlineData(5, 50)]
    public void PaginatedUsersResponse_Should_HandleDifferentPageSizes(int page, int pageSize)
    {
        // Act
        var response = new PaginatedUsersResponse(new List<UserDto>(), 100, page, pageSize);

        // Assert
        response.Page.Should().Be(page);
        response.PageSize.Should().Be(pageSize);
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
            Email: "test@example.com",
            FullName: "Test User",
            Age: 25,
            Gender: "Male",
            University: "Test University",
            ProfilePicturePath: "/images/profile.jpg",
            CreatedAt: createdAt,
            Role: "User"
        );

        // Assert
        dto.Id.Should().Be(id);
        dto.Email.Should().Be("test@example.com");
        dto.FullName.Should().Be("Test User");
        dto.Age.Should().Be(25);
        dto.Gender.Should().Be("Male");
        dto.University.Should().Be("Test University");
        dto.ProfilePicturePath.Should().Be("/images/profile.jpg");
        dto.CreatedAt.Should().Be(createdAt);
        dto.Role.Should().Be("User");
    }

    [Fact]
    public void UserDto_ProfilePicturePath_CanBeNull()
    {
        // Act
        var dto = new UserDto(
            Guid.NewGuid(), "test@test.com", "Test", 20, "F", "Uni", null, DateTime.UtcNow, "User"
        );

        // Assert
        dto.ProfilePicturePath.Should().BeNull();
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Admin")]
    public void UserDto_Should_SupportDifferentRoles(string role)
    {
        // Act
        var dto = new UserDto(
            Guid.NewGuid(), "test@test.com", "Test", 25, "M", "Uni", null, DateTime.UtcNow, role
        );

        // Assert
        dto.Role.Should().Be(role);
    }

    [Fact]
    public void UserDto_Should_SupportRecordEquality()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var dto1 = new UserDto(id, "test@test.com", "Test", 25, "M", "Uni", null, createdAt, "User");
        var dto2 = new UserDto(id, "test@test.com", "Test", 25, "M", "Uni", null, createdAt, "User");

        // Assert
        dto1.Should().Be(dto2);
    }

    #endregion

    #region IProfileService Interface Contract Tests

    [Fact]
    public void IProfileService_Interface_ShouldExist()
    {
        // Assert
        typeof(IProfileService).Should().NotBeNull();
        typeof(IProfileService).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IProfileService_Should_HaveGetAllAsyncMethod()
    {
        // Assert
        var method = typeof(IProfileService).GetMethod("GetAllAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void IProfileService_Should_HaveGetByIdAsyncMethod()
    {
        // Assert
        var method = typeof(IProfileService).GetMethod("GetByIdAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void IProfileService_Should_HaveGetCurrentAsyncMethod()
    {
        // Assert
        var method = typeof(IProfileService).GetMethod("GetCurrentAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void IProfileService_Should_HaveUpdateAsyncMethod()
    {
        // Assert
        var method = typeof(IProfileService).GetMethod("UpdateAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void IProfileService_Should_HaveAdminMethods()
    {
        // Assert
        typeof(IProfileService).GetMethod("GetAdminsAsync").Should().NotBeNull();
        typeof(IProfileService).GetMethod("DeleteProfileAsync").Should().NotBeNull();
        typeof(IProfileService).GetMethod("UpdateRoleAsync").Should().NotBeNull();
        typeof(IProfileService).GetMethod("GetAllUsersAsync").Should().NotBeNull();
    }

    [Fact]
    public void IProfileService_Should_HaveGetUserReviewsMethod()
    {
        // Assert
        var method = typeof(IProfileService).GetMethod("GetUserReviews");
        method.Should().NotBeNull();
    }

    #endregion

    #region IListingService Interface Contract Tests

    [Fact]
    public void IListingService_Interface_ShouldExist()
    {
        // Assert
        typeof(IListingService).Should().NotBeNull();
        typeof(IListingService).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IListingService_Should_HaveSearchAsyncMethod()
    {
        // Assert
        var method = typeof(IListingService).GetMethod("SearchAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void IListingService_Should_HaveGetByIdAsyncMethod()
    {
        // Assert
        var method = typeof(IListingService).GetMethod("GetByIdAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void IListingService_Should_HaveCRUDMethods()
    {
        // Assert
        typeof(IListingService).GetMethod("CreateAsync").Should().NotBeNull();
        typeof(IListingService).GetMethod("UpdateAsync").Should().NotBeNull();
        typeof(IListingService).GetMethod("DeleteAsync").Should().NotBeNull();
    }

    [Fact]
    public void IListingService_Should_HaveApprovalMethods()
    {
        // Assert
        typeof(IListingService).GetMethod("ApproveAsync").Should().NotBeNull();
        typeof(IListingService).GetMethod("RejectAsync").Should().NotBeNull();
    }

    #endregion
}

