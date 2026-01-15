using FluentAssertions;
using RoomMate_Finder.Features.Profiles.Login;
using RoomMate_Finder.Features.Profiles.UpdateProfile;
using RoomMate_Finder.Features.Roommates.GetMyRequests;
using RoomMate_Finder.Features.Roommates.GetRelationships;
using RoomMate_Finder.Features.Roommates.GetPendingRequests;
using RoomMate_Finder.Features.Roommates.SendRequest;

namespace RoomMate_Finder.Test.DTOs;

/// <summary>
/// Tests for DTO records to ensure proper instantiation and equality.
/// </summary>
public class DtoRecordsTests
{
    #region LoginResponse Tests

    [Fact]
    public void LoginResponse_ShouldStoreAllProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string email = "test@example.com";
        const string fullName = "Test User";

        // Act
        var response = new LoginResponse(userId, email, fullName);

        // Assert
        response.UserId.Should().Be(userId);
        response.Email.Should().Be(email);
        response.FullName.Should().Be(fullName);
    }

    [Fact]
    public void LoginResponse_EqualRecords_ShouldBeEqual()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var response1 = new LoginResponse(userId, "test@example.com", "Test User");
        var response2 = new LoginResponse(userId, "test@example.com", "Test User");

        // Act & Assert
        response1.Should().Be(response2);
        response1.GetHashCode().Should().Be(response2.GetHashCode());
    }

    [Fact]
    public void LoginResponse_DifferentRecords_ShouldNotBeEqual()
    {
        // Arrange
        var response1 = new LoginResponse(Guid.NewGuid(), "test1@example.com", "User 1");
        var response2 = new LoginResponse(Guid.NewGuid(), "test2@example.com", "User 2");

        // Act & Assert
        response1.Should().NotBe(response2);
    }

    #endregion

    #region UpdateProfileForm Tests

    [Fact]
    public void UpdateProfileForm_ShouldAllowNullableProperties()
    {
        // Arrange & Act
        var form = new UpdateProfileForm();

        // Assert
        form.FullName.Should().BeNull();
        form.Age.Should().BeNull();
        form.Gender.Should().BeNull();
        form.University.Should().BeNull();
        form.Bio.Should().BeNull();
        form.Lifestyle.Should().BeNull();
        form.Interests.Should().BeNull();
        form.ProfilePicture.Should().BeNull();
    }

    [Fact]
    public void UpdateProfileForm_ShouldStoreSetValues()
    {
        // Arrange & Act
        var form = new UpdateProfileForm
        {
            FullName = "John Doe",
            Age = 25,
            Gender = "Male",
            University = "MIT",
            Bio = "Software Developer",
            Lifestyle = "Active",
            Interests = "Coding, Gaming"
        };

        // Assert
        form.FullName.Should().Be("John Doe");
        form.Age.Should().Be(25);
        form.Gender.Should().Be("Male");
        form.University.Should().Be("MIT");
        form.Bio.Should().Be("Software Developer");
        form.Lifestyle.Should().Be("Active");
        form.Interests.Should().Be("Coding, Gaming");
    }

    [Theory]
    [InlineData("Alice", 20, "Female")]
    [InlineData("Bob", 30, "Male")]
    [InlineData("Charlie", 25, "Other")]
    public void UpdateProfileForm_ShouldAcceptVariousValues(string name, int age, string gender)
    {
        // Arrange & Act
        var form = new UpdateProfileForm
        {
            FullName = name,
            Age = age,
            Gender = gender
        };

        // Assert
        form.FullName.Should().Be(name);
        form.Age.Should().Be(age);
        form.Gender.Should().Be(gender);
    }

    #endregion

    #region MyRequestDto Tests

    [Fact]
    public void MyRequestDto_ShouldStoreAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var processedAt = DateTime.UtcNow.AddHours(1);

        // Act
        var dto = new MyRequestDto(
            id,
            otherUserId,
            "Other User",
            "other@example.com",
            "Please be my roommate",
            "Pending",
            createdAt,
            processedAt
        );

        // Assert
        dto.Id.Should().Be(id);
        dto.OtherUserId.Should().Be(otherUserId);
        dto.OtherUserName.Should().Be("Other User");
        dto.OtherUserEmail.Should().Be("other@example.com");
        dto.Message.Should().Be("Please be my roommate");
        dto.Status.Should().Be("Pending");
        dto.CreatedAt.Should().Be(createdAt);
        dto.ProcessedAt.Should().Be(processedAt);
    }

    [Fact]
    public void MyRequestDto_WithNullMessage_ShouldWork()
    {
        // Arrange & Act
        var dto = new MyRequestDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "User",
            "user@example.com",
            null,
            "Accepted",
            DateTime.UtcNow,
            null
        );

        // Assert
        dto.Message.Should().BeNull();
        dto.ProcessedAt.Should().BeNull();
    }

    #endregion

    #region MyRoommateDto Tests

    [Fact]
    public void MyRoommateDto_ShouldStoreAllProperties()
    {
        // Arrange
        var relationshipId = Guid.NewGuid();
        var roommateId = Guid.NewGuid();
        var since = DateTime.UtcNow;

        // Act
        var dto = new MyRoommateDto(
            relationshipId,
            roommateId,
            "Roommate Name",
            "roommate@example.com",
            since
        );

        // Assert
        dto.RelationshipId.Should().Be(relationshipId);
        dto.RoommateId.Should().Be(roommateId);
        dto.RoommateName.Should().Be("Roommate Name");
        dto.RoommateEmail.Should().Be("roommate@example.com");
        dto.Since.Should().Be(since);
    }

    [Fact]
    public void MyRoommateDto_EqualRecords_ShouldBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var roommateId = Guid.NewGuid();
        var since = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var dto1 = new MyRoommateDto(id, roommateId, "Test", "test@test.com", since);
        var dto2 = new MyRoommateDto(id, roommateId, "Test", "test@test.com", since);

        // Act & Assert
        dto1.Should().Be(dto2);
    }

    #endregion

    #region RoommateRelationshipDto Tests

    [Fact]
    public void RoommateRelationshipDto_ShouldStoreAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var dto = new RoommateRelationshipDto(
            id,
            user1Id,
            "User One",
            "user1@example.com",
            user2Id,
            "User Two",
            "user2@example.com",
            "Admin User",
            createdAt,
            true
        );

        // Assert
        dto.Id.Should().Be(id);
        dto.User1Id.Should().Be(user1Id);
        dto.User1Name.Should().Be("User One");
        dto.User1Email.Should().Be("user1@example.com");
        dto.User2Id.Should().Be(user2Id);
        dto.User2Name.Should().Be("User Two");
        dto.User2Email.Should().Be("user2@example.com");
        dto.ApprovedByAdminName.Should().Be("Admin User");
        dto.CreatedAt.Should().Be(createdAt);
        dto.IsActive.Should().BeTrue();
    }

    [Fact]
    public void RoommateRelationshipDto_InactiveRelationship_ShouldReflectState()
    {
        // Arrange & Act
        var dto = new RoommateRelationshipDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "U1",
            "u1@test.com",
            Guid.NewGuid(),
            "U2",
            "u2@test.com",
            "Admin",
            DateTime.UtcNow,
            false
        );

        // Assert
        dto.IsActive.Should().BeFalse();
    }

    #endregion

    #region PendingRequestDto Tests

    [Fact]
    public void PendingRequestDto_ShouldStoreAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var dto = new PendingRequestDto(
            id,
            requesterId,
            "Requester Name",
            "requester@example.com",
            targetUserId,
            "Target Name",
            "target@example.com",
            "Hi, I'd like to be your roommate!",
            createdAt
        );

        // Assert
        dto.Id.Should().Be(id);
        dto.RequesterId.Should().Be(requesterId);
        dto.RequesterName.Should().Be("Requester Name");
        dto.RequesterEmail.Should().Be("requester@example.com");
        dto.TargetUserId.Should().Be(targetUserId);
        dto.TargetUserName.Should().Be("Target Name");
        dto.TargetUserEmail.Should().Be("target@example.com");
        dto.Message.Should().Be("Hi, I'd like to be your roommate!");
        dto.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void PendingRequestDto_WithNullMessage_ShouldWork()
    {
        // Arrange & Act
        var dto = new PendingRequestDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Req",
            "req@test.com",
            Guid.NewGuid(),
            "Target",
            "target@test.com",
            null,
            DateTime.UtcNow
        );

        // Assert
        dto.Message.Should().BeNull();
    }

    [Fact]
    public void PendingRequestDto_EqualRecords_ShouldBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var date = new DateTime(2024, 6, 15, 10, 0, 0, DateTimeKind.Utc);

        var dto1 = new PendingRequestDto(id, requesterId, "A", "a@t.com", targetId, "B", "b@t.com", "msg", date);
        var dto2 = new PendingRequestDto(id, requesterId, "A", "a@t.com", targetId, "B", "b@t.com", "msg", date);

        // Act & Assert
        dto1.Should().Be(dto2);
        dto1.GetHashCode().Should().Be(dto2.GetHashCode());
    }

    #endregion

    #region GetMyRequestsResponse Tests

    [Fact]
    public void GetMyRequestsResponse_ShouldStoreAllLists()
    {
        // Arrange
        var sentRequests = new List<MyRequestDto>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "User1", "u1@t.com", "msg", "Pending", DateTime.UtcNow, null)
        };
        var receivedRequests = new List<MyRequestDto>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "User2", "u2@t.com", "msg2", "Accepted", DateTime.UtcNow, DateTime.UtcNow)
        };
        var activeRoommates = new List<MyRoommateDto>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Roomie", "r@t.com", DateTime.UtcNow)
        };

        // Act
        var response = new GetMyRequestsResponse(sentRequests, receivedRequests, activeRoommates);

        // Assert
        response.SentRequests.Should().HaveCount(1);
        response.ReceivedRequests.Should().HaveCount(1);
        response.ActiveRoommates.Should().HaveCount(1);
    }

    [Fact]
    public void GetMyRequestsResponse_EmptyLists_ShouldWork()
    {
        // Arrange & Act
        var response = new GetMyRequestsResponse(
            new List<MyRequestDto>(),
            new List<MyRequestDto>(),
            new List<MyRoommateDto>()
        );

        // Assert
        response.SentRequests.Should().BeEmpty();
        response.ReceivedRequests.Should().BeEmpty();
        response.ActiveRoommates.Should().BeEmpty();
    }

    #endregion

    #region GetMyRequestsRequest Tests

    [Fact]
    public void GetMyRequestsRequest_ShouldBeCreatable()
    {
        // Arrange & Act
        var request = new GetMyRequestsRequest();

        // Assert
        request.Should().NotBeNull();
    }

    #endregion

    #region GetRelationshipsRequest Tests

    [Fact]
    public void GetRelationshipsRequest_ShouldBeCreatable()
    {
        // Arrange & Act
        var request = new GetRelationshipsRequest();

        // Assert
        request.Should().NotBeNull();
    }

    #endregion

    #region GetPendingRequestsRequest Tests

    [Fact]
    public void GetPendingRequestsRequest_ShouldBeCreatable()
    {
        // Arrange & Act
        var request = new GetPendingRequestsRequest();

        // Assert
        request.Should().NotBeNull();
    }

    #endregion
}
