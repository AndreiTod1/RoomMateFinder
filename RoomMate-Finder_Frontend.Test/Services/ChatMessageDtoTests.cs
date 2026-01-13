using FluentAssertions;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Services;

public class ChatMessageDtoTests
{
    [Fact]
    public void ChatMessageDto_Should_HaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var sentAt = DateTime.UtcNow;

        // Act
        var dto = new ChatMessageDto(
            Id: id,
            ConversationId: conversationId,
            SenderId: senderId,
            SenderName: "John Doe",
            SenderRole: "User",
            Content: "Hello, how are you?",
            SentAt: sentAt,
            IsRead: false
        );

        // Assert
        dto.Id.Should().Be(id);
        dto.ConversationId.Should().Be(conversationId);
        dto.SenderId.Should().Be(senderId);
        dto.SenderName.Should().Be("John Doe");
        dto.SenderRole.Should().Be("User");
        dto.Content.Should().Be("Hello, how are you?");
        dto.SentAt.Should().Be(sentAt);
        dto.IsRead.Should().BeFalse();
    }

    [Fact]
    public void ChatMessageDto_SenderRole_CanBeNull()
    {
        // Act
        var dto = new ChatMessageDto(
            Id: Guid.NewGuid(),
            ConversationId: Guid.NewGuid(),
            SenderId: Guid.NewGuid(),
            SenderName: "Test User",
            SenderRole: null,
            Content: "Test message",
            SentAt: DateTime.UtcNow,
            IsRead: true
        );

        // Assert
        dto.SenderRole.Should().BeNull();
    }

    [Fact]
    public void ChatMessageDto_Should_SupportAdminRole()
    {
        // Act
        var dto = new ChatMessageDto(
            Id: Guid.NewGuid(),
            ConversationId: Guid.NewGuid(),
            SenderId: Guid.NewGuid(),
            SenderName: "Admin User",
            SenderRole: "Admin",
            Content: "Admin message",
            SentAt: DateTime.UtcNow,
            IsRead: false
        );

        // Assert
        dto.SenderRole.Should().Be("Admin");
    }

    [Fact]
    public void ChatMessageDto_IsRead_Should_BeTrue_WhenRead()
    {
        // Act
        var unreadDto = new ChatMessageDto(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "User", "User", "Unread message", DateTime.UtcNow, false
        );
        var readDto = new ChatMessageDto(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "User", "User", "Read message", DateTime.UtcNow, true
        );

        // Assert
        unreadDto.IsRead.Should().BeFalse();
        readDto.IsRead.Should().BeTrue();
    }

    [Fact]
    public void ChatMessageDto_Should_HandleEmptyContent()
    {
        // Act
        var dto = new ChatMessageDto(
            Id: Guid.NewGuid(),
            ConversationId: Guid.NewGuid(),
            SenderId: Guid.NewGuid(),
            SenderName: "User",
            SenderRole: "User",
            Content: "",
            SentAt: DateTime.UtcNow,
            IsRead: false
        );

        // Assert
        dto.Content.Should().BeEmpty();
    }

    [Fact]
    public void ChatMessageDto_Should_HandleLongContent()
    {
        // Arrange
        var longContent = new string('a', 5000);

        // Act
        var dto = new ChatMessageDto(
            Id: Guid.NewGuid(),
            ConversationId: Guid.NewGuid(),
            SenderId: Guid.NewGuid(),
            SenderName: "User",
            SenderRole: "User",
            Content: longContent,
            SentAt: DateTime.UtcNow,
            IsRead: false
        );

        // Assert
        dto.Content.Length.Should().Be(5000);
    }

    [Fact]
    public void ChatMessageDto_Should_HandleSpecialCharacters()
    {
        // Act
        var dto = new ChatMessageDto(
            Id: Guid.NewGuid(),
            ConversationId: Guid.NewGuid(),
            SenderId: Guid.NewGuid(),
            SenderName: "User 👤",
            SenderRole: "User",
            Content: "Hello! 😀 How are you? <script>alert('xss')</script>",
            SentAt: DateTime.UtcNow,
            IsRead: false
        );

        // Assert
        dto.SenderName.Should().Contain("👤");
        dto.Content.Should().Contain("😀");
        dto.Content.Should().Contain("<script>");
    }

    [Fact]
    public void ChatMessageDto_Should_SupportRecordEquality()
    {
        // Arrange
        var id = Guid.NewGuid();
        var convId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var sentAt = DateTime.UtcNow;

        var dto1 = new ChatMessageDto(id, convId, senderId, "User", "User", "Hello", sentAt, false);
        var dto2 = new ChatMessageDto(id, convId, senderId, "User", "User", "Hello", sentAt, false);

        // Assert
        dto1.Should().Be(dto2);
    }

    [Fact]
    public void ChatMessageDto_Should_NotBeEqual_WhenDifferentContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var convId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var sentAt = DateTime.UtcNow;

        var dto1 = new ChatMessageDto(id, convId, senderId, "User", "User", "Hello", sentAt, false);
        var dto2 = new ChatMessageDto(id, convId, senderId, "User", "User", "Goodbye", sentAt, false);

        // Assert
        dto1.Should().NotBe(dto2);
    }

    [Fact]
    public void ChatMessageDto_SentAt_Should_SupportDifferentTimezones()
    {
        // Arrange
        var utcTime = DateTime.UtcNow;
        var localTime = DateTime.Now;

        // Act
        var utcDto = new ChatMessageDto(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "User", "User", "UTC Message", utcTime, false
        );
        var localDto = new ChatMessageDto(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "User", "User", "Local Message", localTime, false
        );

        // Assert
        utcDto.SentAt.Should().BeCloseTo(utcTime, TimeSpan.FromMilliseconds(1));
        localDto.SentAt.Should().BeCloseTo(localTime, TimeSpan.FromMilliseconds(1));
    }
}

