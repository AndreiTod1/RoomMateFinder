using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Conversations.StartConversation;
using RoomMate_Finder.Test.Helpers;
using Xunit;

namespace RoomMate_Finder.Test.Handlers;

public class StartConversationHandlerTests
{
    [Fact]
    public async Task Given_NonexistentOtherUser_When_HandleIsCalled_Then_KeyNotFoundExceptionIsThrown()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUserId = Guid.NewGuid();
        var httpContextAccessor = HttpContextHelper.CreateMockHttpContextAccessor(currentUserId);
        
        var handler = new StartConversationHandler(context, httpContextAccessor);
        var request = new StartConversationRequest(Guid.NewGuid()); // Non-existent user

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<KeyNotFoundException>();
        ex.Which.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task Given_SameUserAsOtherUserId_When_HandleIsCalled_Then_InvalidOperationExceptionIsThrown()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUserId = Guid.NewGuid();
        var httpContextAccessor = HttpContextHelper.CreateMockHttpContextAccessor(currentUserId);
        
        // Create current user in database
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
        context.Profiles.Add(currentUser);
        await context.SaveChangesAsync();

        var handler = new StartConversationHandler(context, httpContextAccessor);
        var request = new StartConversationRequest(currentUserId); // Same as current user

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Be("Cannot start a conversation with yourself");
    }

    [Fact]
    public async Task Given_ExistingConversation_When_HandleIsCalled_Then_ExistingConversationIsReturned()
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

        // Create existing conversation
        var existingConversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = currentUserId < otherUserId ? currentUserId : otherUserId,
            User2Id = currentUserId < otherUserId ? otherUserId : currentUserId,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        context.Profiles.AddRange(currentUser, otherUser);
        context.Conversations.Add(existingConversation);
        await context.SaveChangesAsync();

        var handler = new StartConversationHandler(context, httpContextAccessor);
        var request = new StartConversationRequest(otherUserId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ConversationId.Should().Be(existingConversation.Id);
        result.User1Id.Should().Be(existingConversation.User1Id);
        result.User2Id.Should().Be(existingConversation.User2Id);
        result.CreatedAt.Should().Be(existingConversation.CreatedAt);
    }

    [Fact]
    public async Task Given_NewConversation_When_HandleIsCalled_Then_NewConversationIsCreatedAndReturned()
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

        context.Profiles.AddRange(currentUser, otherUser);
        await context.SaveChangesAsync();

        var handler = new StartConversationHandler(context, httpContextAccessor);
        var request = new StartConversationRequest(otherUserId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ConversationId.Should().NotBe(Guid.Empty);
        
        // Verify users are stored with lower GUID first
        var expectedUser1Id = currentUserId < otherUserId ? currentUserId : otherUserId;
        var expectedUser2Id = currentUserId < otherUserId ? otherUserId : currentUserId;
        
        result.User1Id.Should().Be(expectedUser1Id);
        result.User2Id.Should().Be(expectedUser2Id);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify conversation was actually saved to database
        var savedConversation = await context.Conversations.FindAsync(result.ConversationId);
        savedConversation.Should().NotBeNull();
        savedConversation!.User1Id.Should().Be(expectedUser1Id);
        savedConversation.User2Id.Should().Be(expectedUser2Id);
    }

    [Fact]
    public async Task Given_ConversationInReverseOrder_When_HandleIsCalled_Then_ExistingConversationIsFound()
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

        // Create conversation in reverse order (other user as User1, current as User2)
        var existingConversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = otherUserId,
            User2Id = currentUserId,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        context.Profiles.AddRange(currentUser, otherUser);
        context.Conversations.Add(existingConversation);
        await context.SaveChangesAsync();

        var handler = new StartConversationHandler(context, httpContextAccessor);
        var request = new StartConversationRequest(otherUserId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ConversationId.Should().Be(existingConversation.Id);
        result.User1Id.Should().Be(existingConversation.User1Id);
        result.User2Id.Should().Be(existingConversation.User2Id);
    }

}
