using FluentAssertions;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Services;

public class ConversationDtoTests
{
    #region ConversationDto Tests

    [Fact]
    public void ConversationDto_Should_HaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var dto = new ConversationDto(
            Id: id,
            OtherUserId: otherUserId,
            OtherUserName: "John Doe",
            OtherUserProfilePicture: "/images/john.jpg",
            OtherUserRole: "User",
            CreatedAt: createdAt
        );

        // Assert
        dto.Id.Should().Be(id);
        dto.OtherUserId.Should().Be(otherUserId);
        dto.OtherUserName.Should().Be("John Doe");
        dto.OtherUserProfilePicture.Should().Be("/images/john.jpg");
        dto.OtherUserRole.Should().Be("User");
        dto.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void ConversationDto_OtherUserProfilePicture_CanBeNull()
    {
        // Act
        var dto = new ConversationDto(
            Id: Guid.NewGuid(),
            OtherUserId: Guid.NewGuid(),
            OtherUserName: "Jane Doe",
            OtherUserProfilePicture: null,
            OtherUserRole: "User",
            CreatedAt: DateTime.UtcNow
        );

        // Assert
        dto.OtherUserProfilePicture.Should().BeNull();
    }

    [Fact]
    public void ConversationDto_OtherUserRole_CanBeNull()
    {
        // Act
        var dto = new ConversationDto(
            Guid.NewGuid(), Guid.NewGuid(), "Test User", null, null, DateTime.UtcNow
        );

        // Assert
        dto.OtherUserRole.Should().BeNull();
    }

    [Fact]
    public void ConversationDto_Should_SupportAdminRole()
    {
        // Act
        var dto = new ConversationDto(
            Guid.NewGuid(), Guid.NewGuid(), "Admin User", "/images/admin.jpg", "Admin", DateTime.UtcNow
        );

        // Assert
        dto.OtherUserRole.Should().Be("Admin");
    }

    #endregion

    #region MessageDto Tests

    [Fact]
    public void MessageDto_Should_HaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var sentAt = DateTime.UtcNow;

        // Act
        var dto = new MessageDto(
            Id: id,
            SenderId: senderId,
            SenderName: "Alice",
            SenderRole: "User",
            Content: "Hello, how are you?",
            SentAt: sentAt,
            IsRead: false
        );

        // Assert
        dto.Id.Should().Be(id);
        dto.SenderId.Should().Be(senderId);
        dto.SenderName.Should().Be("Alice");
        dto.SenderRole.Should().Be("User");
        dto.Content.Should().Be("Hello, how are you?");
        dto.SentAt.Should().Be(sentAt);
        dto.IsRead.Should().BeFalse();
    }

    [Fact]
    public void MessageDto_SenderRole_CanBeNull()
    {
        // Act
        var dto = new MessageDto(
            Guid.NewGuid(), Guid.NewGuid(), "User", null, "Message", DateTime.UtcNow, true
        );

        // Assert
        dto.SenderRole.Should().BeNull();
    }

    [Fact]
    public void MessageDto_IsRead_Should_TrackReadStatus()
    {
        // Act
        var unread = new MessageDto(
            Guid.NewGuid(), Guid.NewGuid(), "User", "User", "Unread", DateTime.UtcNow, false
        );
        var read = new MessageDto(
            Guid.NewGuid(), Guid.NewGuid(), "User", "User", "Read", DateTime.UtcNow, true
        );

        // Assert
        unread.IsRead.Should().BeFalse();
        read.IsRead.Should().BeTrue();
    }

    [Fact]
    public void MessageDto_Should_HandleEmptyContent()
    {
        // Act
        var dto = new MessageDto(
            Guid.NewGuid(), Guid.NewGuid(), "User", "User", "", DateTime.UtcNow, false
        );

        // Assert
        dto.Content.Should().BeEmpty();
    }

    [Fact]
    public void MessageDto_Should_HandleLongContent()
    {
        // Arrange
        var longContent = new string('x', 10000);

        // Act
        var dto = new MessageDto(
            Guid.NewGuid(), Guid.NewGuid(), "User", "User", longContent, DateTime.UtcNow, false
        );

        // Assert
        dto.Content.Length.Should().Be(10000);
    }

    #endregion

    #region UnreadConversationsResponse Tests

    [Fact]
    public void UnreadConversationsResponse_Should_HaveCorrectProperties()
    {
        // Arrange
        var convId1 = Guid.NewGuid();
        var convId2 = Guid.NewGuid();
        var unreadList = new List<UnreadConversationDto>
        {
            new UnreadConversationDto(convId1, 5),
            new UnreadConversationDto(convId2, 3)
        };

        // Act
        var response = new UnreadConversationsResponse(
            UnreadConversations: unreadList,
            TotalUnreadMessages: 8
        );

        // Assert
        response.UnreadConversations.Should().HaveCount(2);
        response.TotalUnreadMessages.Should().Be(8);
    }

    [Fact]
    public void UnreadConversationsResponse_Should_HandleEmptyList()
    {
        // Act
        var response = new UnreadConversationsResponse(
            UnreadConversations: new List<UnreadConversationDto>(),
            TotalUnreadMessages: 0
        );

        // Assert
        response.UnreadConversations.Should().BeEmpty();
        response.TotalUnreadMessages.Should().Be(0);
    }

    #endregion

    #region UnreadConversationDto Tests

    [Fact]
    public void UnreadConversationDto_Should_HaveCorrectProperties()
    {
        // Arrange
        var conversationId = Guid.NewGuid();

        // Act
        var dto = new UnreadConversationDto(
            ConversationId: conversationId,
            UnreadCount: 10
        );

        // Assert
        dto.ConversationId.Should().Be(conversationId);
        dto.UnreadCount.Should().Be(10);
    }

    [Fact]
    public void UnreadConversationDto_UnreadCount_ShouldBeZeroOrPositive()
    {
        // Act
        var zeroUnread = new UnreadConversationDto(Guid.NewGuid(), 0);
        var someUnread = new UnreadConversationDto(Guid.NewGuid(), 100);

        // Assert
        zeroUnread.UnreadCount.Should().Be(0);
        someUnread.UnreadCount.Should().Be(100);
    }

    #endregion
}

