using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Conversations.GetUnreadConversations;
using RoomMate_Finder.Test.Helpers;

namespace RoomMate_Finder.Test.Handlers;

public class GetUnreadConversationsHandlerTests
{
    private static IHttpContextAccessor CreateHttpContextAccessor(Guid userId)
    {
        var mockAccessor = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Items["CurrentUserId"] = userId;
        mockAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        return mockAccessor.Object;
    }

    [Fact]
    public async Task Given_NoUnreadMessages_When_GetUnreadConversationsIsCalled_Then_ReturnsEmptyList()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var userId = Guid.NewGuid();
        var httpContextAccessor = CreateHttpContextAccessor(userId);
        var handler = new GetUnreadConversationsHandler(context, httpContextAccessor);

        // Act
        var result = await handler.Handle(new GetUnreadConversationsRequest(), CancellationToken.None);

        // Assert
        result.UnreadConversations.Should().BeEmpty();
        result.TotalUnreadMessages.Should().Be(0);
    }

    [Fact]
    public async Task Given_UnreadMessagesExist_When_GetUnreadConversationsIsCalled_Then_ReturnsCorrectCount()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        
        var user1 = new Profile
        {
            Id = currentUserId,
            Email = "current@test.com",
            PasswordHash = "hash",
            FullName = "Current User",
            Age = 25,
            Gender = "M",
            CreatedAt = DateTime.UtcNow
        };
        var user2 = new Profile
        {
            Id = otherUserId,
            Email = "other@test.com",
            PasswordHash = "hash",
            FullName = "Other User",
            Age = 26,
            Gender = "F",
            CreatedAt = DateTime.UtcNow
        };
        
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = currentUserId,
            User2Id = otherUserId,
            CreatedAt = DateTime.UtcNow
        };
        
        // Add unread messages from other user
        var unreadMessage1 = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = otherUserId,
            Content = "Hello!",
            SentAt = DateTime.UtcNow,
            IsRead = false
        };
        var unreadMessage2 = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = otherUserId,
            Content = "Are you there?",
            SentAt = DateTime.UtcNow.AddMinutes(1),
            IsRead = false
        };
        
        context.Profiles.AddRange(user1, user2);
        context.Conversations.Add(conversation);
        context.Messages.AddRange(unreadMessage1, unreadMessage2);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateHttpContextAccessor(currentUserId);
        var handler = new GetUnreadConversationsHandler(context, httpContextAccessor);

        // Act
        var result = await handler.Handle(new GetUnreadConversationsRequest(), CancellationToken.None);

        // Assert
        result.UnreadConversations.Should().HaveCount(1);
        result.UnreadConversations[0].UnreadCount.Should().Be(2);
        result.TotalUnreadMessages.Should().Be(2);
    }

    [Fact]
    public async Task Given_OwnMessages_When_GetUnreadConversationsIsCalled_Then_DoesNotCountOwnMessages()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        
        var user1 = new Profile
        {
            Id = currentUserId,
            Email = "current@test.com",
            PasswordHash = "hash",
            FullName = "Current User",
            Age = 25,
            Gender = "M",
            CreatedAt = DateTime.UtcNow
        };
        var user2 = new Profile
        {
            Id = otherUserId,
            Email = "other@test.com",
            PasswordHash = "hash",
            FullName = "Other User",
            Age = 26,
            Gender = "F",
            CreatedAt = DateTime.UtcNow
        };
        
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = currentUserId,
            User2Id = otherUserId,
            CreatedAt = DateTime.UtcNow
        };
        
        // Add unread message from current user (should not count)
        var ownMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = currentUserId,
            Content = "My own message",
            SentAt = DateTime.UtcNow,
            IsRead = false
        };
        
        context.Profiles.AddRange(user1, user2);
        context.Conversations.Add(conversation);
        context.Messages.Add(ownMessage);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateHttpContextAccessor(currentUserId);
        var handler = new GetUnreadConversationsHandler(context, httpContextAccessor);

        // Act
        var result = await handler.Handle(new GetUnreadConversationsRequest(), CancellationToken.None);

        // Assert
        result.UnreadConversations.Should().BeEmpty();
        result.TotalUnreadMessages.Should().Be(0);
    }

    [Fact]
    public async Task Given_ReadMessages_When_GetUnreadConversationsIsCalled_Then_DoesNotCountReadMessages()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        
        var user1 = new Profile
        {
            Id = currentUserId,
            Email = "current@test.com",
            PasswordHash = "hash",
            FullName = "Current User",
            Age = 25,
            Gender = "M",
            CreatedAt = DateTime.UtcNow
        };
        var user2 = new Profile
        {
            Id = otherUserId,
            Email = "other@test.com",
            PasswordHash = "hash",
            FullName = "Other User",
            Age = 26,
            Gender = "F",
            CreatedAt = DateTime.UtcNow
        };
        
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = currentUserId,
            User2Id = otherUserId,
            CreatedAt = DateTime.UtcNow
        };
        
        // Add read message from other user
        var readMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = otherUserId,
            Content = "Already read message",
            SentAt = DateTime.UtcNow,
            IsRead = true
        };
        
        context.Profiles.AddRange(user1, user2);
        context.Conversations.Add(conversation);
        context.Messages.Add(readMessage);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateHttpContextAccessor(currentUserId);
        var handler = new GetUnreadConversationsHandler(context, httpContextAccessor);

        // Act
        var result = await handler.Handle(new GetUnreadConversationsRequest(), CancellationToken.None);

        // Assert
        result.UnreadConversations.Should().BeEmpty();
        result.TotalUnreadMessages.Should().Be(0);
    }

    [Fact]
    public async Task Given_MultipleConversations_When_GetUnreadConversationsIsCalled_Then_ReturnsAllWithUnread()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUserId = Guid.NewGuid();
        var otherUser1Id = Guid.NewGuid();
        var otherUser2Id = Guid.NewGuid();
        
        var currentUser = new Profile
        {
            Id = currentUserId,
            Email = "current@test.com",
            PasswordHash = "hash",
            FullName = "Current User",
            Age = 25,
            Gender = "M",
            CreatedAt = DateTime.UtcNow
        };
        var otherUser1 = new Profile
        {
            Id = otherUser1Id,
            Email = "other1@test.com",
            PasswordHash = "hash",
            FullName = "Other User 1",
            Age = 26,
            Gender = "F",
            CreatedAt = DateTime.UtcNow
        };
        var otherUser2 = new Profile
        {
            Id = otherUser2Id,
            Email = "other2@test.com",
            PasswordHash = "hash",
            FullName = "Other User 2",
            Age = 27,
            Gender = "M",
            CreatedAt = DateTime.UtcNow
        };
        
        var conversation1 = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = currentUserId,
            User2Id = otherUser1Id,
            CreatedAt = DateTime.UtcNow
        };
        var conversation2 = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = currentUserId,
            User2Id = otherUser2Id,
            CreatedAt = DateTime.UtcNow
        };
        
        var msg1 = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation1.Id,
            SenderId = otherUser1Id,
            Content = "Message 1",
            SentAt = DateTime.UtcNow,
            IsRead = false
        };
        var msg2 = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation2.Id,
            SenderId = otherUser2Id,
            Content = "Message 2",
            SentAt = DateTime.UtcNow,
            IsRead = false
        };
        var msg3 = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation2.Id,
            SenderId = otherUser2Id,
            Content = "Message 3",
            SentAt = DateTime.UtcNow,
            IsRead = false
        };
        
        context.Profiles.AddRange(currentUser, otherUser1, otherUser2);
        context.Conversations.AddRange(conversation1, conversation2);
        context.Messages.AddRange(msg1, msg2, msg3);
        await context.SaveChangesAsync();

        var httpContextAccessor = CreateHttpContextAccessor(currentUserId);
        var handler = new GetUnreadConversationsHandler(context, httpContextAccessor);

        // Act
        var result = await handler.Handle(new GetUnreadConversationsRequest(), CancellationToken.None);

        // Assert
        result.UnreadConversations.Should().HaveCount(2);
        result.TotalUnreadMessages.Should().Be(3);
    }
}
