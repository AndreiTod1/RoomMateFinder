using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Conversations.GetConversations;
using RoomMate_Finder.Test.Helpers;
using Xunit;

namespace RoomMate_Finder.Test.Handlers;

public class GetConversationsHandlerTests : IDisposable
{
    private bool _disposed;

    [Fact]
    public async Task Given_NoConversations_When_HandleIsCalled_Then_EmptyListIsReturned()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUserId = Guid.NewGuid();
        var httpContextAccessor = HttpContextHelper.CreateMockHttpContextAccessor(currentUserId);
        
        var handler = new GetConversationsHandler(context, httpContextAccessor);
        var request = new GetConversationsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Conversations.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_ConversationsWhereUserIsUser1_When_HandleIsCalled_Then_ConversationsAreReturned()
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
            User1Id = currentUserId, // Current user is User1
            User2Id = otherUserId,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        context.Profiles.AddRange(currentUser, otherUser);
        context.Conversations.Add(conversation);
        await context.SaveChangesAsync();

        var handler = new GetConversationsHandler(context, httpContextAccessor);
        var request = new GetConversationsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Conversations.Should().HaveCount(1);
        
        var conversationDto = result.Conversations[0];
        conversationDto.Id.Should().Be(conversation.Id);
        conversationDto.OtherUserId.Should().Be(otherUserId);
        conversationDto.OtherUserName.Should().Be("Other User");
        conversationDto.CreatedAt.Should().Be(conversation.CreatedAt);
    }

    [Fact]
    public async Task Given_ConversationsWhereUserIsUser2_When_HandleIsCalled_Then_ConversationsAreReturned()
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
            User1Id = otherUserId,
            User2Id = currentUserId, // Current user is User2
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        context.Profiles.AddRange(currentUser, otherUser);
        context.Conversations.Add(conversation);
        await context.SaveChangesAsync();

        var handler = new GetConversationsHandler(context, httpContextAccessor);
        var request = new GetConversationsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Conversations.Should().HaveCount(1);
        
        var conversationDto = result.Conversations[0];
        conversationDto.Id.Should().Be(conversation.Id);
        conversationDto.OtherUserId.Should().Be(otherUserId);
        conversationDto.OtherUserName.Should().Be("Other User");
        conversationDto.CreatedAt.Should().Be(conversation.CreatedAt);
    }

    [Fact]
    public async Task Given_MultipleConversations_When_HandleIsCalled_Then_ConversationsAreOrderedByCreatedAtDescending()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUserId = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
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
        
        var user1 = new Profile
        {
            Id = user1Id,
            Email = "user1@example.com",
            PasswordHash = "hashedpass1",
            FullName = "User One",
            Age = 26,
            Gender = "F",
            University = "Test University 1",
            Bio = "Bio 1",
            Lifestyle = "Calm",
            Interests = "Reading",
            CreatedAt = DateTime.UtcNow
        };
        
        var user2 = new Profile
        {
            Id = user2Id,
            Email = "user2@example.com",
            PasswordHash = "hashedpass2",
            FullName = "User Two",
            Age = 27,
            Gender = "M",
            University = "Test University 2",
            Bio = "Bio 2",
            Lifestyle = "Social",
            Interests = "Gaming",
            CreatedAt = DateTime.UtcNow
        };

        // Create conversations with different created times
        var conversation1 = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = currentUserId,
            User2Id = user1Id,
            CreatedAt = DateTime.UtcNow.AddMinutes(-20) // Older
        };

        var conversation2 = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = user2Id,
            User2Id = currentUserId,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5) // Newer
        };

        context.Profiles.AddRange(currentUser, user1, user2);
        context.Conversations.AddRange(conversation1, conversation2);
        await context.SaveChangesAsync();

        var handler = new GetConversationsHandler(context, httpContextAccessor);
        var request = new GetConversationsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Conversations.Should().HaveCount(2);
        
        // Should be ordered by CreatedAt descending (newest first)
        result.Conversations[0].Id.Should().Be(conversation2.Id); // Newer conversation
        result.Conversations[0].OtherUserName.Should().Be("User Two");
        
        result.Conversations[1].Id.Should().Be(conversation1.Id); // Older conversation
        result.Conversations[1].OtherUserName.Should().Be("User One");
    }

    [Fact]
    public async Task Given_ConversationsNotInvolvingCurrentUser_When_HandleIsCalled_Then_EmptyListIsReturned()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUserId = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var httpContextAccessor = HttpContextHelper.CreateMockHttpContextAccessor(currentUserId);
        
        // Create users
        var user1 = new Profile
        {
            Id = user1Id,
            Email = "user1@example.com",
            PasswordHash = "hashedpass1",
            FullName = "User One",
            Age = 26,
            Gender = "F",
            University = "Test University 1",
            Bio = "Bio 1",
            Lifestyle = "Calm",
            Interests = "Reading",
            CreatedAt = DateTime.UtcNow
        };
        
        var user2 = new Profile
        {
            Id = user2Id,
            Email = "user2@example.com",
            PasswordHash = "hashedpass2",
            FullName = "User Two",
            Age = 27,
            Gender = "M",
            University = "Test University 2",
            Bio = "Bio 2",
            Lifestyle = "Social",
            Interests = "Gaming",
            CreatedAt = DateTime.UtcNow
        };

        // Create conversation between other users (not involving current user)
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = user1Id,
            User2Id = user2Id,
            CreatedAt = DateTime.UtcNow
        };

        context.Profiles.AddRange(user1, user2);
        context.Conversations.Add(conversation);
        await context.SaveChangesAsync();

        var handler = new GetConversationsHandler(context, httpContextAccessor);
        var request = new GetConversationsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Conversations.Should().BeEmpty();
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
