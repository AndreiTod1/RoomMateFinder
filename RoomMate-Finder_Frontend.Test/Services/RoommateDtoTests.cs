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
        var message = "Request sent successfully";

        // Act
        var response = new SendRoommateRequestResponse(id, message);

        // Assert
        response.Id.Should().Be(id);
        response.Message.Should().Be(message);
    }

    [Fact]
    public void SendRoommateRequestResponse_Should_SupportRecordEquality()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response1 = new SendRoommateRequestResponse(id, "Message");
        var response2 = new SendRoommateRequestResponse(id, "Message");

        // Assert
        response1.Should().Be(response2);
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
        var processedAt = DateTime.UtcNow.AddHours(1);

        // Act
        var dto = new MyRequestDto(
            Id: id,
            OtherUserId: otherUserId,
            OtherUserName: "John Doe",
            OtherUserEmail: "john@example.com",
            Message: "Looking for a roommate",
            Status: "Pending",
            CreatedAt: createdAt,
            ProcessedAt: processedAt
        );

        // Assert
        dto.Id.Should().Be(id);
        dto.OtherUserId.Should().Be(otherUserId);
        dto.OtherUserName.Should().Be("John Doe");
        dto.OtherUserEmail.Should().Be("john@example.com");
        dto.Message.Should().Be("Looking for a roommate");
        dto.Status.Should().Be("Pending");
        dto.CreatedAt.Should().Be(createdAt);
        dto.ProcessedAt.Should().Be(processedAt);
    }

    [Fact]
    public void MyRequestDto_Message_CanBeNull()
    {
        // Act
        var dto = new MyRequestDto(
            Guid.NewGuid(), Guid.NewGuid(), "User", "user@test.com", 
            null, "Pending", DateTime.UtcNow, null
        );

        // Assert
        dto.Message.Should().BeNull();
    }

    [Fact]
    public void MyRequestDto_ProcessedAt_CanBeNull()
    {
        // Act
        var dto = new MyRequestDto(
            Guid.NewGuid(), Guid.NewGuid(), "User", "user@test.com",
            "Message", "Pending", DateTime.UtcNow, null
        );

        // Assert
        dto.ProcessedAt.Should().BeNull();
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("MutuallyConfirmed")]
    [InlineData("Approved")]
    [InlineData("Rejected")]
    public void MyRequestDto_Should_SupportVariousStatuses(string status)
    {
        // Act
        var dto = new MyRequestDto(
            Guid.NewGuid(), Guid.NewGuid(), "User", "user@test.com",
            null, status, DateTime.UtcNow, null
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
        var since = DateTime.UtcNow.AddDays(-30);

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

    [Fact]
    public void MyRoommateDto_Should_SupportRecordEquality()
    {
        // Arrange
        var relationshipId = Guid.NewGuid();
        var roommateId = Guid.NewGuid();
        var since = DateTime.UtcNow;

        var dto1 = new MyRoommateDto(relationshipId, roommateId, "Jane", "jane@test.com", since);
        var dto2 = new MyRoommateDto(relationshipId, roommateId, "Jane", "jane@test.com", since);

        // Assert
        dto1.Should().Be(dto2);
    }

    #endregion

    #region MyRoommateRequestsResponse Tests

    [Fact]
    public void MyRoommateRequestsResponse_Should_HaveCorrectProperties()
    {
        // Arrange
        var sentRequests = new List<MyRequestDto>
        {
            new MyRequestDto(Guid.NewGuid(), Guid.NewGuid(), "User1", "user1@test.com", null, "Pending", DateTime.UtcNow, null)
        };
        var receivedRequests = new List<MyRequestDto>
        {
            new MyRequestDto(Guid.NewGuid(), Guid.NewGuid(), "User2", "user2@test.com", "Hi!", "Pending", DateTime.UtcNow, null)
        };
        var activeRoommates = new List<MyRoommateDto>
        {
            new MyRoommateDto(Guid.NewGuid(), Guid.NewGuid(), "Roommate1", "roommate@test.com", DateTime.UtcNow.AddDays(-10))
        };

        // Act
        var response = new MyRoommateRequestsResponse(sentRequests, receivedRequests, activeRoommates);

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
}

