using FluentAssertions;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Services;

public class ConversationServiceExtendedTests
{
    #region MessageDto Extended Tests

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
            SenderName: "John Doe",
            SenderRole: "User",
            Content: "Hello, is this room still available?",
            SentAt: sentAt,
            IsRead: false
        );

        // Assert
        dto.Id.Should().Be(id);
        dto.SenderId.Should().Be(senderId);
        dto.SenderName.Should().Be("John Doe");
        dto.SenderRole.Should().Be("User");
        dto.Content.Should().Be("Hello, is this room still available?");
        dto.SentAt.Should().Be(sentAt);
        dto.IsRead.Should().BeFalse();
    }

    [Fact]
    public void MessageDto_SenderRole_CanBeNull()
    {
        // Act
        var dto = new MessageDto(
            Guid.NewGuid(), Guid.NewGuid(), "User", null, "Content", DateTime.UtcNow, false
        );

        // Assert
        dto.SenderRole.Should().BeNull();
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Admin")]
    [InlineData(null)]
    public void MessageDto_Should_SupportDifferentRoles(string? role)
    {
        // Act
        var dto = new MessageDto(
            Guid.NewGuid(), Guid.NewGuid(), "User", role, "Content", DateTime.UtcNow, true
        );

        // Assert
        dto.SenderRole.Should().Be(role);
    }

    [Fact]
    public void MessageDto_IsRead_ShouldBeCorrect()
    {
        // Act
        var unreadMessage = new MessageDto(
            Guid.NewGuid(), Guid.NewGuid(), "User", "User", "Message 1", DateTime.UtcNow, false
        );
        var readMessage = new MessageDto(
            Guid.NewGuid(), Guid.NewGuid(), "User", "User", "Message 2", DateTime.UtcNow, true
        );

        // Assert
        unreadMessage.IsRead.Should().BeFalse();
        readMessage.IsRead.Should().BeTrue();
    }

    [Fact]
    public void MessageDto_Should_SupportRecordEquality()
    {
        // Arrange
        var id = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var sentAt = DateTime.UtcNow;

        var dto1 = new MessageDto(id, senderId, "User", "User", "Hello", sentAt, false);
        var dto2 = new MessageDto(id, senderId, "User", "User", "Hello", sentAt, false);

        // Assert
        dto1.Should().Be(dto2);
    }

    #endregion

    #region UnreadConversationsResponse Tests

    [Fact]
    public void UnreadConversationsResponse_Should_HaveCorrectProperties()
    {
        // Arrange
        var unreadConversations = new List<UnreadConversationDto>
        {
            new UnreadConversationDto(Guid.NewGuid(), 5),
            new UnreadConversationDto(Guid.NewGuid(), 3),
            new UnreadConversationDto(Guid.NewGuid(), 2)
        };

        // Act
        var response = new UnreadConversationsResponse(unreadConversations, 10);

        // Assert
        response.UnreadConversations.Should().HaveCount(3);
        response.TotalUnreadMessages.Should().Be(10);
    }

    [Fact]
    public void UnreadConversationsResponse_Should_HandleEmptyList()
    {
        // Act
        var response = new UnreadConversationsResponse(new List<UnreadConversationDto>(), 0);

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
        var dto = new UnreadConversationDto(conversationId, 7);

        // Assert
        dto.ConversationId.Should().Be(conversationId);
        dto.UnreadCount.Should().Be(7);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void UnreadConversationDto_Should_AcceptVariousUnreadCounts(int count)
    {
        // Act
        var dto = new UnreadConversationDto(Guid.NewGuid(), count);

        // Assert
        dto.UnreadCount.Should().Be(count);
    }

    #endregion

    #region IConversationService Interface Contract Tests

    [Fact]
    public void IConversationService_Interface_ShouldExist()
    {
        // Assert
        typeof(IConversationService).Should().NotBeNull();
        typeof(IConversationService).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IConversationService_Should_HaveGetConversationsAsyncMethod()
    {
        // Assert
        var method = typeof(IConversationService).GetMethod("GetConversationsAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void IConversationService_Should_HaveGetMessagesAsyncMethod()
    {
        // Assert
        var method = typeof(IConversationService).GetMethod("GetMessagesAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void IConversationService_Should_HaveStartConversationAsyncMethod()
    {
        // Assert
        var method = typeof(IConversationService).GetMethod("StartConversationAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void IConversationService_Should_HaveSendMessageAsyncMethod()
    {
        // Assert
        var method = typeof(IConversationService).GetMethod("SendMessageAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void IConversationService_Should_HaveMarkMessagesAsReadAsyncMethod()
    {
        // Assert
        var method = typeof(IConversationService).GetMethod("MarkMessagesAsReadAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void IConversationService_Should_HaveGetUnreadConversationsAsyncMethod()
    {
        // Assert
        var method = typeof(IConversationService).GetMethod("GetUnreadConversationsAsync");
        method.Should().NotBeNull();
    }

    #endregion
}

