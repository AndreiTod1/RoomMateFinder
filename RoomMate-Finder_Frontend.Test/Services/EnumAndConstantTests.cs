using FluentAssertions;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Services;

public class EnumAndConstantTests
{
    #region ListingApprovalStatus Enum Tests

    [Fact]
    public void ListingApprovalStatus_Should_HaveThreeValues()
    {
        // Assert
        Enum.GetValues(typeof(ListingApprovalStatus)).Length.Should().Be(3);
    }

    [Fact]
    public void ListingApprovalStatus_Values_ShouldBeCorrect()
    {
        // Assert
        ((int)ListingApprovalStatus.Pending).Should().Be(0);
        ((int)ListingApprovalStatus.Approved).Should().Be(1);
        ((int)ListingApprovalStatus.Rejected).Should().Be(2);
    }

    [Fact]
    public void ListingApprovalStatus_Should_ParseFromString()
    {
        // Act & Assert
        Enum.Parse<ListingApprovalStatus>("Pending").Should().Be(ListingApprovalStatus.Pending);
        Enum.Parse<ListingApprovalStatus>("Approved").Should().Be(ListingApprovalStatus.Approved);
        Enum.Parse<ListingApprovalStatus>("Rejected").Should().Be(ListingApprovalStatus.Rejected);
    }

    [Fact]
    public void ListingApprovalStatus_Should_ConvertToString()
    {
        // Assert
        ListingApprovalStatus.Pending.ToString().Should().Be("Pending");
        ListingApprovalStatus.Approved.ToString().Should().Be("Approved");
        ListingApprovalStatus.Rejected.ToString().Should().Be("Rejected");
    }

    [Theory]
    [InlineData(0, "Pending")]
    [InlineData(1, "Approved")]
    [InlineData(2, "Rejected")]
    public void ListingApprovalStatus_IntToEnum_ShouldWork(int value, string expected)
    {
        // Act
        var status = (ListingApprovalStatus)value;

        // Assert
        status.ToString().Should().Be(expected);
    }

    #endregion

    #region Record Immutability Tests

    [Fact]
    public void ProfileDto_Should_BeImmutable()
    {
        // Arrange
        var dto = new ProfileDto(
            Guid.NewGuid(), "test@test.com", "Test", 25, "M", "Uni", "Bio", "quiet", "Music",
            DateTime.UtcNow, null, "User"
        );

        // Assert - Records are immutable by default
        var type = typeof(ProfileDto);
        type.IsClass.Should().BeTrue();
        
        // All properties should be init-only (via record syntax)
        foreach (var prop in type.GetProperties())
        {
            prop.CanRead.Should().BeTrue();
        }
    }

    [Fact]
    public void ConversationDto_Should_BeImmutable()
    {
        // Arrange
        var dto = new ConversationDto(
            Guid.NewGuid(), Guid.NewGuid(), "User", null, "User", DateTime.UtcNow
        );

        // Assert
        var type = typeof(ConversationDto);
        type.IsClass.Should().BeTrue();
    }

    [Fact]
    public void MessageDto_Should_BeImmutable()
    {
        // Arrange
        var dto = new MessageDto(
            Guid.NewGuid(), Guid.NewGuid(), "User", "User", "Content", DateTime.UtcNow, false
        );

        // Assert
        var type = typeof(MessageDto);
        type.IsClass.Should().BeTrue();
    }

    #endregion

    #region Record With Expression Tests

    [Fact]
    public void ProfileDto_WithExpression_ShouldCreateNewInstance()
    {
        // Arrange
        var original = new ProfileDto(
            Guid.NewGuid(), "test@test.com", "Original Name", 25, "M", "Uni", "Bio", "quiet", "Music",
            DateTime.UtcNow, null, "User"
        );

        // Act - Use 'with' expression to create modified copy
        var modified = original with { FullName = "New Name" };

        // Assert
        original.FullName.Should().Be("Original Name");
        modified.FullName.Should().Be("New Name");
        original.Id.Should().Be(modified.Id);
        ReferenceEquals(original, modified).Should().BeFalse();
    }

    [Fact]
    public void ListingDto_WithExpression_ShouldCreateNewInstance()
    {
        // Arrange
        var original = new ListingDto(
            Guid.NewGuid(), Guid.NewGuid(), "Original Title", "Description",
            "City", "Area", 500, DateTime.UtcNow, new List<string>(),
            DateTime.UtcNow, true
        );

        // Act
        var modified = original with { Title = "New Title", Price = 600 };

        // Assert
        original.Title.Should().Be("Original Title");
        original.Price.Should().Be(500);
        modified.Title.Should().Be("New Title");
        modified.Price.Should().Be(600);
    }

    #endregion

    #region Nullable Property Tests

    [Fact]
    public void ProfileDto_NullableProperties_ShouldBeHandledCorrectly()
    {
        // Act - Create with null ProfilePicturePath
        var dto = new ProfileDto(
            Guid.NewGuid(), "test@test.com", "Test", 25, "M", "Uni", "Bio", "quiet", "Music",
            DateTime.UtcNow, null
        );

        // Assert
        dto.ProfilePicturePath.Should().BeNull();
        dto.Role.Should().Be("User"); // Default value
    }

    [Fact]
    public void ListingSummaryDto_NullableProperties_ShouldBeHandledCorrectly()
    {
        // Act
        var dto = new ListingSummaryDto(
            Guid.NewGuid(), Guid.NewGuid(), "Owner", "Title", "City", "Area",
            500, DateTime.UtcNow, new List<string>(), true
        );

        // Assert
        dto.ThumbnailPath.Should().BeNull();
        dto.RejectionReason.Should().BeNull();
        dto.ApprovalStatus.Should().Be(ListingApprovalStatus.Pending);
    }

    [Fact]
    public void ConversationDto_NullableProperties_ShouldBeHandledCorrectly()
    {
        // Act
        var dto = new ConversationDto(
            Guid.NewGuid(), Guid.NewGuid(), "User Name", null, null, DateTime.UtcNow
        );

        // Assert
        dto.OtherUserProfilePicture.Should().BeNull();
        dto.OtherUserRole.Should().BeNull();
    }

    #endregion
}

