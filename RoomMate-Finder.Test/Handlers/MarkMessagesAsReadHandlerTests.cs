using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Conversations.MarkMessagesAsRead;
using RoomMate_Finder.Test.Helpers;
using Xunit;

namespace RoomMate_Finder.Test.Handlers;

public class MarkMessagesAsReadHandlerTests
{
    [Fact]
    public async Task Given_NonexistentConversation_When_HandleIsCalled_Then_KeyNotFoundExceptionIsThrown()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUserId = Guid.NewGuid();
        var httpContextAccessor = HttpContextHelper.CreateMockHttpContextAccessor(currentUserId);
        
        var handler = new MarkMessagesAsReadHandler(context, httpContextAccessor);
        var request = new MarkMessagesAsReadRequest(Guid.NewGuid());

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<KeyNotFoundException>();
        ex.Which.Message.Should().Contain("Conversation with ID");
        ex.Which.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task Given_UserNotParticipantInConversation_When_HandleIsCalled_Then_UnauthorizedAccessExceptionIsThrown()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUserId = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var httpContextAccessor = HttpContextHelper.CreateMockHttpContextAccessor(currentUserId);
        
        // Create a conversation between two other users
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = user1Id,
            User2Id = user2Id,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Conversations.Add(conversation);
        await context.SaveChangesAsync();

        var handler = new MarkMessagesAsReadHandler(context, httpContextAccessor);
        var request = new MarkMessagesAsReadRequest(conversation.Id);

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<UnauthorizedAccessException>();
        ex.Which.Message.Should().Be("You are not a participant in this conversation");
    }

    [Fact]
    public async Task Given_NoUnreadMessages_When_HandleIsCalled_Then_ZeroMessagesAreMarkedAsRead()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var httpContextAccessor = HttpContextHelper.CreateMockHttpContextAccessor(currentUserId);
        
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = currentUserId,
            User2Id = otherUserId,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Conversations.Add(conversation);
        await context.SaveChangesAsync();

        var handler = new MarkMessagesAsReadHandler(context, httpContextAccessor);
        var request = new MarkMessagesAsReadRequest(conversation.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.MessagesMarkedAsRead.Should().Be(0);
    }

    [Fact]
    public async Task Given_UnreadMessagesFromOtherUser_When_HandleIsCalled_Then_MessagesAreMarkedAsRead()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var httpContextAccessor = HttpContextHelper.CreateMockHttpContextAccessor(currentUserId);
        
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = currentUserId,
            User2Id = otherUserId,
            CreatedAt = DateTime.UtcNow
        };

        // Create messages from other user (unread)
        var message1 = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = otherUserId, // From other user
            Content = "Message 1",
            SentAt = DateTime.UtcNow.AddMinutes(-10),
            IsRead = false // Unread
        };

        var message2 = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = otherUserId, // From other user
            Content = "Message 2",
            SentAt = DateTime.UtcNow.AddMinutes(-5),
            IsRead = false // Unread
        };

        // Create message from current user (should not be marked)
        var ownMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = currentUserId, // From current user
            Content = "Own message",
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        context.Conversations.Add(conversation);
        context.Messages.AddRange(message1, message2, ownMessage);
        await context.SaveChangesAsync();

        var handler = new MarkMessagesAsReadHandler(context, httpContextAccessor);
        var request = new MarkMessagesAsReadRequest(conversation.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.MessagesMarkedAsRead.Should().Be(2); // Only messages from other user

        // Verify messages are actually marked as read
        var updatedMessages = await context.Messages
            .Where(m => m.ConversationId == conversation.Id)
            .ToListAsync();
        
        var updatedMessage1 = updatedMessages.First(m => m.Id == message1.Id);
        var updatedMessage2 = updatedMessages.First(m => m.Id == message2.Id);
        var updatedOwnMessage = updatedMessages.First(m => m.Id == ownMessage.Id);
        
        updatedMessage1.IsRead.Should().BeTrue();
        updatedMessage2.IsRead.Should().BeTrue();
        updatedOwnMessage.IsRead.Should().BeFalse(); // Own message should remain unchanged
    }

    [Fact]
    public async Task Given_AlreadyReadMessages_When_HandleIsCalled_Then_OnlyUnreadMessagesAreMarked()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var httpContextAccessor = HttpContextHelper.CreateMockHttpContextAccessor(currentUserId);
        
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = currentUserId,
            User2Id = otherUserId,
            CreatedAt = DateTime.UtcNow
        };

        // Create already read message
        var readMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = otherUserId,
            Content = "Already read message",
            SentAt = DateTime.UtcNow.AddMinutes(-20),
            IsRead = true // Already read
        };

        // Create unread message
        var unreadMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = otherUserId,
            Content = "Unread message",
            SentAt = DateTime.UtcNow.AddMinutes(-10),
            IsRead = false // Unread
        };

        context.Conversations.Add(conversation);
        context.Messages.AddRange(readMessage, unreadMessage);
        await context.SaveChangesAsync();

        var handler = new MarkMessagesAsReadHandler(context, httpContextAccessor);
        var request = new MarkMessagesAsReadRequest(conversation.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.MessagesMarkedAsRead.Should().Be(1); // Only the unread message

        // Verify correct message was marked
        var updatedUnreadMessage = await context.Messages.FindAsync(unreadMessage.Id);
        updatedUnreadMessage!.IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task Given_UserIsUser2InConversation_When_HandleIsCalled_Then_MessagesAreMarkedCorrectly()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var httpContextAccessor = HttpContextHelper.CreateMockHttpContextAccessor(currentUserId);
        
        // Create conversation where current user is User2
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = otherUserId,
            User2Id = currentUserId, // Current user is User2
            CreatedAt = DateTime.UtcNow
        };

        var unreadMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = otherUserId, // From User1
            Content = "Message from User1",
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        context.Conversations.Add(conversation);
        context.Messages.Add(unreadMessage);
        await context.SaveChangesAsync();

        var handler = new MarkMessagesAsReadHandler(context, httpContextAccessor);
        var request = new MarkMessagesAsReadRequest(conversation.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.MessagesMarkedAsRead.Should().Be(1);

        var updatedMessage = await context.Messages.FindAsync(unreadMessage.Id);
        updatedMessage!.IsRead.Should().BeTrue();
    }

}
