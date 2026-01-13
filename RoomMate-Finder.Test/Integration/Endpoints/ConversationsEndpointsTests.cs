using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RoomMate_Finder.Features.Conversations.SendMessage;
using RoomMate_Finder.Features.Conversations.StartConversation;
using RoomMate_Finder.Features.Conversations.GetConversations;
using RoomMate_Finder.Features.Conversations.GetMessages;
using RoomMate_Finder.Test.Integration;
using Xunit;
using RoomMate_Finder.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using RoomMate_Finder.Common;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Conversations.MarkMessagesAsRead;
using RoomMate_Finder.Features.Conversations.GetUnreadConversations;

namespace RoomMate_Finder.Test.Integration.Endpoints;

public class ConversationsEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ConversationsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Profile User, string Token)> CreateUserAndGetTokenAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var jwtService = scope.ServiceProvider.GetRequiredService<JwtService>();

        var user = new Profile
        {
            Id = Guid.NewGuid(),
            Email = $"test_{Guid.NewGuid()}@example.com",
            PasswordHash = "hashedpass",
            FullName = "Test User",
            CreatedAt = DateTime.UtcNow,
            Role = "User"
        };

        db.Profiles.Add(user);
        await db.SaveChangesAsync();

        var token = jwtService.GenerateToken(user);
        return (user, token);
    }

    [Fact]
    public async Task StartConversation_ShouldReturnOk_WhenAuthorized()
    {
        // Arrange
        var (user, token) = await CreateUserAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var otherUserId = Guid.NewGuid();
        context.Profiles.Add(new Profile { Id = otherUserId, Email = "other@test.com", FullName = "Other", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var request = new { OtherUserId = otherUserId };

        // Act
        var response = await _client.PostAsJsonAsync("/conversations", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<StartConversationResponse>();
        result.Should().NotBeNull();
        result!.ConversationId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SendMessage_ShouldReturnOk_WhenConversationExists()
    {
        // Arrange
        var (user, token) = await CreateUserAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var otherUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        
        context.Profiles.Add(new Profile { Id = otherUserId, Email = "other2@test.com", FullName = "Other 2", CreatedAt = DateTime.UtcNow });
        context.Conversations.Add(new Conversation 
        { 
            Id = conversationId, 
            User1Id = user.Id, 
            User2Id = otherUserId,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var requestBody = new SendMessageRequestBody("Integration Test Message");

        // Act
        var response = await _client.PostAsJsonAsync($"/conversations/{conversationId}/messages", requestBody);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SendMessageResponse>();
        result!.Content.Should().Be("Integration Test Message");
    }

    [Fact]
    public async Task GetConversations_ShouldReturnList_WhenAuthorized()
    {
        // Arrange
        var (user, token) = await CreateUserAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/conversations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetConversationsResponse>();
        result.Should().NotBeNull();
        result!.Conversations.Should().NotBeNull();
    }
    [Fact]
    public async Task GetMessages_ShouldReturnList_WhenAuthorized()
    {
        // Arrange
        var (user, token) = await CreateUserAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var otherUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        
        context.Profiles.Add(new Profile { Id = otherUserId, Email = "other_msgs@test.com", FullName = "Other M", CreatedAt = DateTime.UtcNow });
        context.Conversations.Add(new Conversation 
        { 
            Id = conversationId, 
            User1Id = user.Id, 
            User2Id = otherUserId,
            CreatedAt = DateTime.UtcNow
        });
        context.Messages.Add(new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = otherUserId,
            Content = "Hello",
            SentAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/conversations/{conversationId}/messages");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetMessagesResponse>();
        result.Should().NotBeNull();
        result!.Messages.Should().ContainSingle();
        result.Messages.First().Content.Should().Be("Hello");
    }

    [Fact]
    public async Task MarkMessagesAsRead_ShouldReturnOk()
    {
        // Arrange
        var (user, token) = await CreateUserAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var otherUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        
        context.Profiles.Add(new Profile { Id = otherUserId, Email = "other_read@test.com", FullName = "Other R", CreatedAt = DateTime.UtcNow });
        context.Conversations.Add(new Conversation 
        { 
            Id = conversationId, 
            User1Id = user.Id, 
            User2Id = otherUserId,
            CreatedAt = DateTime.UtcNow
        });
        context.Messages.Add(new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = otherUserId,
            Content = "Unread msg",
            SentAt = DateTime.UtcNow,
            IsRead = false
        });
        await context.SaveChangesAsync();

        // Act
        var response = await _client.PutAsync($"/conversations/{conversationId}/messages/mark-read", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<MarkMessagesAsReadResponse>();
        result.Should().NotBeNull();
        result!.MessagesMarkedAsRead.Should().Be(1);
    }

    [Fact]
    public async Task GetUnreadConversations_ShouldReturnList()
    {
        // Arrange
        var (user, token) = await CreateUserAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var otherUserId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        
        context.Profiles.Add(new Profile { Id = otherUserId, Email = "other_unread@test.com", FullName = "Other U", CreatedAt = DateTime.UtcNow });
        context.Conversations.Add(new Conversation 
        { 
            Id = conversationId, 
            User1Id = user.Id, 
            User2Id = otherUserId,
            CreatedAt = DateTime.UtcNow
        });
        // Message from OTHER user to CURRENT user (unread)
        context.Messages.Add(new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = otherUserId,
            Content = "Unread",
            SentAt = DateTime.UtcNow,
            IsRead = false
        });
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/conversations/unread");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetUnreadConversationsResponse>();
        result.Should().NotBeNull();
        result!.UnreadConversations.Should().ContainSingle();
        result.UnreadConversations.First().ConversationId.Should().Be(conversationId);
        result.UnreadConversations.First().UnreadCount.Should().Be(1);
    }
}
