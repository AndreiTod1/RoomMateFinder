using FluentAssertions;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Services;

public class RoommateDtoTests
{
    #region SendRoommateRequestResponse Tests

    [Fact]
    public void SendRoommateRequestResponse_Should_HaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = new SendRoommateRequestResponse(
            Id: id,
            Message: "Request sent successfully"
        );

        // Assert
        response.Id.Should().Be(id);
        response.Message.Should().Be("Request sent successfully");
    }

    #endregion

    #region MyRoommateRequestsResponse Tests

    [Fact]
    public void MyRoommateRequestsResponse_Should_HaveCorrectProperties()
    {
        // Arrange
        var sentRequests = new List<MyRequestDto>
        {
            new MyRequestDto(Guid.NewGuid(), Guid.NewGuid(), "User 1", "user1@test.com", "Want to be roommates?", "Pending", DateTime.UtcNow, null)
        };
        var receivedRequests = new List<MyRequestDto>
        {
            new MyRequestDto(Guid.NewGuid(), Guid.NewGuid(), "User 2", "user2@test.com", null, "MutuallyConfirmed", DateTime.UtcNow.AddDays(-1), null)
        };
        var activeRoommates = new List<MyRoommateDto>
        {
            new MyRoommateDto(Guid.NewGuid(), Guid.NewGuid(), "Roommate 1", "roommate1@test.com", DateTime.UtcNow.AddMonths(-3))
        };

        // Act
        var response = new MyRoommateRequestsResponse(
            SentRequests: sentRequests,
            ReceivedRequests: receivedRequests,
            ActiveRoommates: activeRoommates
        );

        // Assert
        response.SentRequests.Should().HaveCount(1);
        response.ReceivedRequests.Should().HaveCount(1);
        response.ActiveRoommates.Should().HaveCount(1);
    }

    [Fact]
    public void MyRoommateRequestsResponse_Should_HandleEmptyLists()
    {
        // Act
        var response = new MyRoommateRequestsResponse(
            SentRequests: new List<MyRequestDto>(),
            ReceivedRequests: new List<MyRequestDto>(),
            ActiveRoommates: new List<MyRoommateDto>()
        );

        // Assert
        response.SentRequests.Should().BeEmpty();
        response.ReceivedRequests.Should().BeEmpty();
        response.ActiveRoommates.Should().BeEmpty();
    }

    #endregion

    #region MyRequestDto Tests

    [Fact]
    public void MyRequestDto_Should_HaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var processedAt = DateTime.UtcNow.AddHours(2);

        // Act
        var dto = new MyRequestDto(
            Id: id,
            OtherUserId: otherUserId,
            OtherUserName: "John Doe",
            OtherUserEmail: "john@example.com",
            Message: "Let's be roommates!",
            Status: "Approved",
            CreatedAt: createdAt,
            ProcessedAt: processedAt
        );

        // Assert
        dto.Id.Should().Be(id);
        dto.OtherUserId.Should().Be(otherUserId);
        dto.OtherUserName.Should().Be("John Doe");
        dto.OtherUserEmail.Should().Be("john@example.com");
        dto.Message.Should().Be("Let's be roommates!");
        dto.Status.Should().Be("Approved");
        dto.CreatedAt.Should().Be(createdAt);
        dto.ProcessedAt.Should().Be(processedAt);
    }

    [Fact]
    public void MyRequestDto_Message_CanBeNull()
    {
        // Act
        var dto = new MyRequestDto(
            Guid.NewGuid(), Guid.NewGuid(), "User", "user@test.com", null, "Pending", DateTime.UtcNow, null
        );

        // Assert
        dto.Message.Should().BeNull();
    }

    [Fact]
    public void MyRequestDto_ProcessedAt_CanBeNull()
    {
        // Act
        var dto = new MyRequestDto(
            Guid.NewGuid(), Guid.NewGuid(), "User", "user@test.com", "Message", "Pending", DateTime.UtcNow, null
        );

        // Assert
        dto.ProcessedAt.Should().BeNull();
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("MutuallyConfirmed")]
    [InlineData("Approved")]
    [InlineData("Rejected")]
    public void MyRequestDto_Status_ShouldAcceptValidStatuses(string status)
    {
        // Act
        var dto = new MyRequestDto(
            Guid.NewGuid(), Guid.NewGuid(), "User", "user@test.com", null, status, DateTime.UtcNow, null
        );

        // Assert
        dto.Status.Should().Be(status);
    }

    #endregion

    #region MyRoommateDto Tests

    [Fact]
    public void MyRoommateDto_Should_HaveCorrectProperties()
    {
        // Arrange
        var relationshipId = Guid.NewGuid();
        var roommateId = Guid.NewGuid();
        var since = DateTime.UtcNow.AddMonths(-6);

        // Act
        var dto = new MyRoommateDto(
            RelationshipId: relationshipId,
            RoommateId: roommateId,
            RoommateName: "Jane Smith",
            RoommateEmail: "jane@example.com",
            Since: since
        );

        // Assert
        dto.RelationshipId.Should().Be(relationshipId);
        dto.RoommateId.Should().Be(roommateId);
        dto.RoommateName.Should().Be("Jane Smith");
        dto.RoommateEmail.Should().Be("jane@example.com");
        dto.Since.Should().Be(since);
    }

    #endregion

    #region PendingRoommateRequestDto Tests

    [Fact]
    public void PendingRoommateRequestDto_Should_HaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var dto = new PendingRoommateRequestDto(
            Id: id,
            RequesterId: requesterId,
            RequesterName: "Alice",
            RequesterEmail: "alice@test.com",
            TargetUserId: targetUserId,
            TargetUserName: "Bob",
            TargetUserEmail: "bob@test.com",
            Message: "We'd be great roommates!",
            CreatedAt: createdAt
        );

        // Assert
        dto.Id.Should().Be(id);
        dto.RequesterId.Should().Be(requesterId);
        dto.RequesterName.Should().Be("Alice");
        dto.RequesterEmail.Should().Be("alice@test.com");
        dto.TargetUserId.Should().Be(targetUserId);
        dto.TargetUserName.Should().Be("Bob");
        dto.TargetUserEmail.Should().Be("bob@test.com");
        dto.Message.Should().Be("We'd be great roommates!");
        dto.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void PendingRoommateRequestDto_Message_CanBeNull()
    {
        // Act
        var dto = new PendingRoommateRequestDto(
            Guid.NewGuid(), Guid.NewGuid(), "Requester", "req@test.com",
            Guid.NewGuid(), "Target", "target@test.com", null, DateTime.UtcNow
        );

        // Assert
        dto.Message.Should().BeNull();
    }

    #endregion

    #region ApproveRequestResponse Tests

    [Fact]
    public void ApproveRequestResponse_Should_HaveCorrectProperties()
    {
        // Arrange
        var relationshipId = Guid.NewGuid();

        // Act
        var response = new ApproveRequestResponse(
            RelationshipId: relationshipId,
            Message: "Request approved successfully"
        );

        // Assert
        response.RelationshipId.Should().Be(relationshipId);
        response.Message.Should().Be("Request approved successfully");
    }

    #endregion

    #region RejectRequestResponse Tests

    [Fact]
    public void RejectRequestResponse_Should_HaveCorrectProperties()
    {
        // Act
        var response = new RejectRequestResponse(
            Message: "Request rejected"
        );

        // Assert
        response.Message.Should().Be("Request rejected");
    }

    #endregion

    #region RoommateRelationshipDto Tests

    [Fact]
    public void RoommateRelationshipDto_Should_HaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var dto = new RoommateRelationshipDto(
            Id: id,
            User1Id: user1Id,
            User1Name: "User One",
            User1Email: "user1@test.com",
            User2Id: user2Id,
            User2Name: "User Two",
            User2Email: "user2@test.com",
            ApprovedByAdminName: "Admin User",
            CreatedAt: createdAt,
            IsActive: true
        );

        // Assert
        dto.Id.Should().Be(id);
        dto.User1Id.Should().Be(user1Id);
        dto.User1Name.Should().Be("User One");
        dto.User1Email.Should().Be("user1@test.com");
        dto.User2Id.Should().Be(user2Id);
        dto.User2Name.Should().Be("User Two");
        dto.User2Email.Should().Be("user2@test.com");
        dto.ApprovedByAdminName.Should().Be("Admin User");
        dto.CreatedAt.Should().Be(createdAt);
        dto.IsActive.Should().BeTrue();
    }

    [Fact]
    public void RoommateRelationshipDto_IsActive_CanBeFalse()
    {
        // Act
        var dto = new RoommateRelationshipDto(
            Guid.NewGuid(), Guid.NewGuid(), "User1", "u1@test.com",
            Guid.NewGuid(), "User2", "u2@test.com", "Admin", DateTime.UtcNow, false
        );

        // Assert
        dto.IsActive.Should().BeFalse();
    }

    #endregion

    #region DeleteRelationshipResponse Tests

    [Fact]
    public void DeleteRelationshipResponse_Should_HaveCorrectProperties()
    {
        // Act
        var response = new DeleteRelationshipResponse(
            Message: "Relationship deleted successfully"
        );

        // Assert
        response.Message.Should().Be("Relationship deleted successfully");
    }

    #endregion

    #region UserRoommateDto Tests

    [Fact]
    public void UserRoommateDto_Should_HaveCorrectProperties()
    {
        // Arrange
        var relationshipId = Guid.NewGuid();
        var roommateId = Guid.NewGuid();
        var since = DateTime.UtcNow.AddMonths(-2);

        // Act
        var dto = new UserRoommateDto(
            RelationshipId: relationshipId,
            RoommateId: roommateId,
            RoommateName: "My Roommate",
            RoommateEmail: "roommate@test.com",
            ProfilePicturePath: "/images/roommate.jpg",
            Age: 25,
            University: "Test University",
            Since: since
        );

        // Assert
        dto.RelationshipId.Should().Be(relationshipId);
        dto.RoommateId.Should().Be(roommateId);
        dto.RoommateName.Should().Be("My Roommate");
        dto.RoommateEmail.Should().Be("roommate@test.com");
        dto.ProfilePicturePath.Should().Be("/images/roommate.jpg");
        dto.Age.Should().Be(25);
        dto.University.Should().Be("Test University");
        dto.Since.Should().Be(since);
    }

    [Fact]
    public void UserRoommateDto_ProfilePicturePath_CanBeNull()
    {
        // Act
        var dto = new UserRoommateDto(
            Guid.NewGuid(), Guid.NewGuid(), "Roommate", "rm@test.com", null, 30, "University", DateTime.UtcNow
        );

        // Assert
        dto.ProfilePicturePath.Should().BeNull();
    }

    [Fact]
    public void UserRoommateDto_University_CanBeNull()
    {
        // Act
        var dto = new UserRoommateDto(
            Guid.NewGuid(), Guid.NewGuid(), "Roommate", "rm@test.com", "/img/rm.jpg", 28, null, DateTime.UtcNow
        );

        // Assert
        dto.University.Should().BeNull();
    }

    #endregion
}

