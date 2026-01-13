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
}
