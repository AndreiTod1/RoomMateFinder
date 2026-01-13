using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Conversations.GetMessages;
using RoomMate_Finder.Test.Helpers;
using Xunit;

namespace RoomMate_Finder.Test.Handlers;

public class GetMessagesHandlerTests : IDisposable
{
    private bool _disposed;

    [Fact]
    public async Task Given_NonexistentConversation_When_HandleIsCalled_Then_KeyNotFoundExceptionIsThrown()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUserId = Guid.NewGuid();
        var httpContextAccessor = HttpContextHelper.CreateMockHttpContextAccessor(currentUserId);
        
        var handler = new GetMessagesHandler(context, httpContextAccessor);
        var request = new GetMessagesRequest(Guid.NewGuid());

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

        var handler = new GetMessagesHandler(context, httpContextAccessor);
        var request = new GetMessagesRequest(conversation.Id);

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<UnauthorizedAccessException>();
        ex.Which.Message.Should().Be("You are not a participant in this conversation");
    }

    [Fact]
    public async Task Given_ConversationWithNoMessages_When_HandleIsCalled_Then_EmptyListIsReturned()
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

        var handler = new GetMessagesHandler(context, httpContextAccessor);
        var request = new GetMessagesRequest(conversation.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_ConversationWithMessages_When_HandleIsCalled_Then_MessagesAreReturnedOrderedBySentAt()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var httpContextAccessor = HttpContextHelper.CreateMockHttpContextAccessor(currentUserId);
        
        // Create users
        var currentUser = new Profile
        {
            Id = currentUserId,
            Email = "current@example.com",
            PasswordHash = "hashedpass",
            FullName = "Current User",
            Age = 25,
            Gender = "M",
            University = "Test University",
            Bio = "Bio",
            Lifestyle = "Active",
            Interests = "Sports",
            CreatedAt = DateTime.UtcNow
        };
        
        var otherUser = new Profile
        {
            Id = otherUserId,
            Email = "other@example.com",
            PasswordHash = "hashedpass2",
            FullName = "Other User",
            Age = 26,
            Gender = "F",
            University = "Test University 2",
            Bio = "Bio 2",
            Lifestyle = "Calm",
            Interests = "Reading",
            CreatedAt = DateTime.UtcNow
        };
        
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = currentUserId,
            User2Id = otherUserId,
            CreatedAt = DateTime.UtcNow
        };

        // Create messages with different sent times
        var message1 = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = currentUserId,
            Content = "First message",
            SentAt = DateTime.UtcNow.AddMinutes(-20),
            IsRead = true
        };

        var message2 = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = otherUserId,
            Content = "Second message",
            SentAt = DateTime.UtcNow.AddMinutes(-10),
            IsRead = false
        };

        var message3 = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = currentUserId,
            Content = "Third message",
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        context.Profiles.AddRange(currentUser, otherUser);
        context.Conversations.Add(conversation);
        context.Messages.AddRange(message1, message2, message3);
        await context.SaveChangesAsync();

        var handler = new GetMessagesHandler(context, httpContextAccessor);
        var request = new GetMessagesRequest(conversation.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Messages.Should().HaveCount(3);
        
        // Should be ordered by SentAt ascending (oldest first)
        result.Messages[0].Id.Should().Be(message1.Id);
        result.Messages[0].SenderId.Should().Be(currentUserId);
        result.Messages[0].SenderName.Should().Be("Current User");
        result.Messages[0].Content.Should().Be("First message");
        result.Messages[0].IsRead.Should().BeTrue();
        
        result.Messages[1].Id.Should().Be(message2.Id);
        result.Messages[1].SenderId.Should().Be(otherUserId);
        result.Messages[1].SenderName.Should().Be("Other User");
        result.Messages[1].Content.Should().Be("Second message");
        result.Messages[1].IsRead.Should().BeFalse();
        
        result.Messages[2].Id.Should().Be(message3.Id);
        result.Messages[2].SenderId.Should().Be(currentUserId);
        result.Messages[2].SenderName.Should().Be("Current User");
        result.Messages[2].Content.Should().Be("Third message");
        result.Messages[2].IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task Given_UserIsUser2InConversation_When_HandleIsCalled_Then_MessagesAreReturnedSuccessfully()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var httpContextAccessor = HttpContextHelper.CreateMockHttpContextAccessor(currentUserId);
        
        // Create users
        var currentUser = new Profile
        {
            Id = currentUserId,
            Email = "current@example.com",
            PasswordHash = "hashedpass",
            FullName = "Current User",
            Age = 25,
            Gender = "M",
            University = "Test University",
            Bio = "Bio",
            Lifestyle = "Active",
            Interests = "Sports",
            CreatedAt = DateTime.UtcNow
        };
        
        var otherUser = new Profile
        {
            Id = otherUserId,
            Email = "other@example.com",
            PasswordHash = "hashedpass2",
            FullName = "Other User",
            Age = 26,
            Gender = "F",
            University = "Test University 2",
            Bio = "Bio 2",
            Lifestyle = "Calm",
            Interests = "Reading",
            CreatedAt = DateTime.UtcNow
        };
        
        // Create conversation where current user is User2
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = otherUserId,
            User2Id = currentUserId, // Current user is User2
            CreatedAt = DateTime.UtcNow
        };

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = otherUserId,
            Content = "Hello from other user",
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        context.Profiles.AddRange(currentUser, otherUser);
        context.Conversations.Add(conversation);
        context.Messages.Add(message);
        await context.SaveChangesAsync();

        var handler = new GetMessagesHandler(context, httpContextAccessor);
        var request = new GetMessagesRequest(conversation.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Messages.Should().HaveCount(1);
        result.Messages[0].Content.Should().Be("Hello from other user");
        result.Messages[0].SenderName.Should().Be("Other User");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources if any
                // Currently no managed resources to dispose in this test class
            }

            // Dispose unmanaged resources (if any)
            
            _disposed = true;
        }
    }
}
