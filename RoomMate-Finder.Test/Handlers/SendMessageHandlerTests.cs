using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Conversations.SendMessage;
using RoomMate_Finder.Test.Helpers;
using Xunit;

namespace RoomMate_Finder.Test.Handlers;

public class SendMessageHandlerTests : IDisposable
{
    private bool _disposed;

    [Fact]
    public async Task Given_NonexistentConversation_When_HandleIsCalled_Then_KeyNotFoundExceptionIsThrown()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUserId = Guid.NewGuid();
        var httpContextAccessor = HttpContextHelper.CreateMockHttpContextAccessor(currentUserId);
        
        var handler = new SendMessageHandler(context, httpContextAccessor);
        var request = new SendMessageRequest(Guid.NewGuid(), "Hello, this is a test message!");

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

        var handler = new SendMessageHandler(context, httpContextAccessor);
        var request = new SendMessageRequest(conversation.Id, "This should not be allowed!");

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<UnauthorizedAccessException>();
        ex.Which.Message.Should().Be("You are not a participant in this conversation");
    }

    [Fact]
    public async Task Given_ValidRequest_When_HandleIsCalled_Then_MessageIsCreatedAndSaved()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var httpContextAccessor = HttpContextHelper.CreateMockHttpContextAccessor(currentUserId);
        
        // Create conversation where current user is a participant
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = currentUserId,
            User2Id = otherUserId,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Conversations.Add(conversation);
        await context.SaveChangesAsync();

        var handler = new SendMessageHandler(context, httpContextAccessor);
        var messageContent = "Hello, this is a test message!";
        var request = new SendMessageRequest(conversation.Id, messageContent);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.MessageId.Should().NotBe(Guid.Empty);
        result.ConversationId.Should().Be(conversation.Id);
        result.SenderId.Should().Be(currentUserId);
        result.Content.Should().Be(messageContent);
        result.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify message was actually saved to database
        var savedMessage = await context.Messages.FindAsync(result.MessageId);
        savedMessage.Should().NotBeNull();
        savedMessage!.ConversationId.Should().Be(conversation.Id);
        savedMessage.SenderId.Should().Be(currentUserId);
        savedMessage.Content.Should().Be(messageContent);
        savedMessage.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task Given_UserIsUser2InConversation_When_HandleIsCalled_Then_MessageIsCreatedSuccessfully()
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
            User2Id = currentUserId,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Conversations.Add(conversation);
        await context.SaveChangesAsync();

        var handler = new SendMessageHandler(context, httpContextAccessor);
        var messageContent = "Hello from User2!";
        var request = new SendMessageRequest(conversation.Id, messageContent);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ConversationId.Should().Be(conversation.Id);
        result.SenderId.Should().Be(currentUserId);
        result.Content.Should().Be(messageContent);

        // Verify in database
        var savedMessage = await context.Messages.FindAsync(result.MessageId);
        savedMessage.Should().NotBeNull();
        savedMessage!.SenderId.Should().Be(currentUserId);
    }

    [Fact]
    public async Task Given_EmptyMessageContent_When_HandleIsCalled_Then_MessageIsStillCreated()
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

        var handler = new SendMessageHandler(context, httpContextAccessor);
        var request = new SendMessageRequest(conversation.Id, ""); // Empty content

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().BeEmpty();

        var savedMessage = await context.Messages.FindAsync(result.MessageId);
        savedMessage.Should().NotBeNull();
        savedMessage!.Content.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_LongMessageContent_When_HandleIsCalled_Then_MessageIsCreatedWithFullContent()
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

        var handler = new SendMessageHandler(context, httpContextAccessor);
        var longContent = new string('A', 1000); // Very long message
        var request = new SendMessageRequest(conversation.Id, longContent);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Content.Should().Be(longContent);
        result.Content.Should().HaveLength(1000);

        var savedMessage = await context.Messages.FindAsync(result.MessageId);
        savedMessage.Should().NotBeNull();
        savedMessage!.Content.Should().Be(longContent);
    }

    [Fact]
    public async Task Given_MultipleMessagesInSameConversation_When_HandleIsCalled_Then_AllMessagesAreSavedCorrectly()
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

        var handler = new SendMessageHandler(context, httpContextAccessor);

        // Act - Send multiple messages
        var result1 = await handler.Handle(new SendMessageRequest(conversation.Id, "First message"), CancellationToken.None);
        var result2 = await handler.Handle(new SendMessageRequest(conversation.Id, "Second message"), CancellationToken.None);
        var result3 = await handler.Handle(new SendMessageRequest(conversation.Id, "Third message"), CancellationToken.None);

        // Assert
        result1.Content.Should().Be("First message");
        result2.Content.Should().Be("Second message");
        result3.Content.Should().Be("Third message");

        // Verify all messages are in database
        var allMessages = await context.Messages
            .Where(m => m.ConversationId == conversation.Id)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        allMessages.Should().HaveCount(3);
        allMessages[0].Content.Should().Be("First message");
        allMessages[1].Content.Should().Be("Second message");
        allMessages[2].Content.Should().Be("Third message");
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
