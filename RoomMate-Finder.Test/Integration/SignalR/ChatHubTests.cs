using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RoomMate_Finder.Common;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Hubs;
using RoomMate_Finder.Infrastructure.Persistence;
using Xunit;

namespace RoomMate_Finder.Test.Integration.SignalR;

public class ChatHubTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly JwtService _jwtService;
    private AppDbContext _dbContext = null!;
    private Profile _user1 = null!;
    private Profile _user2 = null!;
    private Conversation _conversation = null!;

    public ChatHubTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _jwtService = new JwtService(
            "TestJwtKeyForIntegrationTesting123456789012345678901234567890",
            "TestIssuer",
            "TestAudience"
        );
    }

    public async Task InitializeAsync()
    {
        // Get a fresh DbContext for each test
        var scope = _factory.Services.CreateScope();
        _dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create test users
        _user1 = new Profile
        {
            Id = Guid.NewGuid(),
            FullName = "Test User 1",
            Email = "user1@test.com",
            PasswordHash = "hash",
            Age = 25,
            Gender = "Male",
            University = "Test Uni",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        _user2 = new Profile
        {
            Id = Guid.NewGuid(),
            FullName = "Test User 2",
            Email = "user2@test.com",
            PasswordHash = "hash",
            Age = 26,
            Gender = "Female",
            University = "Test Uni",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        _conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = _user1.Id,
            User2Id = _user2.Id,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Profiles.AddRange(_user1, _user2);
        _dbContext.Conversations.Add(_conversation);
        await _dbContext.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        _dbContext?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ChatHub_Connect_AuthenticatedUser_SuccessfullyConnects()
    {
        // Arrange
        var token = _jwtService.GenerateToken(_user1);
        var connection = CreateHubConnection(token);

        try
        {
            // Act
            await connection.StartAsync();

            // Assert
            Assert.Equal(HubConnectionState.Connected, connection.State);
        }
        finally
        {
            await connection.StopAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task ChatHub_Connect_UnauthenticatedUser_FailsToConnect()
    {
        // Arrange
        var connection = CreateHubConnection(null);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await connection.StartAsync();
        });

        await connection.DisposeAsync();
    }

    [Fact]
    public async Task ChatHub_JoinConversation_UserInConversation_JoinsSuccessfully()
    {
        // Arrange
        var token = _jwtService.GenerateToken(_user1);
        var connection = CreateHubConnection(token);

        try
        {
            await connection.StartAsync();

            // Act
            await connection.InvokeAsync("JoinConversation", _conversation.Id);

            // Assert - No exception means success
            Assert.Equal(HubConnectionState.Connected, connection.State);
        }
        finally
        {
            await connection.StopAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task ChatHub_SendMessage_ValidMessage_BroadcastsToGroup()
    {
        // Arrange
        var token1 = _jwtService.GenerateToken(_user1);
        var token2 = _jwtService.GenerateToken(_user2);
        
        var connection1 = CreateHubConnection(token1);
        var connection2 = CreateHubConnection(token2);

        ChatMessageDto? receivedMessage = null;
        var messageReceivedTcs = new TaskCompletionSource<bool>();

        connection2.On<ChatMessageDto>("ReceiveMessage", (message) =>
        {
            receivedMessage = message;
            messageReceivedTcs.TrySetResult(true);
        });

        try
        {
            await connection1.StartAsync();
            await connection2.StartAsync();

            // Both users join the conversation
            await connection1.InvokeAsync("JoinConversation", _conversation.Id);
            await connection2.InvokeAsync("JoinConversation", _conversation.Id);

            // Act
            var messageContent = "Hello from User 1!";
            await connection1.InvokeAsync("SendMessage", _conversation.Id, messageContent);

            // Wait for message to be received
            await Task.WhenAny(messageReceivedTcs.Task, Task.Delay(5000));

            // Assert
            Assert.NotNull(receivedMessage);
            Assert.Equal(messageContent, receivedMessage!.Content);
            Assert.Equal(_user1.Id, receivedMessage.SenderId);
            Assert.Equal(_user1.FullName, receivedMessage.SenderName);
            Assert.Equal(_conversation.Id, receivedMessage.ConversationId);

            // Verify message was saved to database
            var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var savedMessage = await dbContext.Messages
                .FirstOrDefaultAsync(m => m.Content == messageContent);
            
            Assert.NotNull(savedMessage);
            Assert.Equal(_user1.Id, savedMessage!.SenderId);
            Assert.Equal(_conversation.Id, savedMessage.ConversationId);
        }
        finally
        {
            await connection1.StopAsync();
            await connection2.StopAsync();
            await connection1.DisposeAsync();
            await connection2.DisposeAsync();
        }
    }

    [Fact]
    public async Task ChatHub_SendMessage_UserNotInConversation_MessageNotSent()
    {
        // Arrange
        var unauthorizedUser = new Profile
        {
            Id = Guid.NewGuid(),
            FullName = "Unauthorized User",
            Email = "unauthorized@test.com",
            PasswordHash = "hash",
            Age = 30,
            Gender = "Male",
            University = "Test Uni",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Profiles.Add(unauthorizedUser);
        await _dbContext.SaveChangesAsync();

        var token = _jwtService.GenerateToken(unauthorizedUser);
        var connection = CreateHubConnection(token);

        try
        {
            await connection.StartAsync();

            // Act
            await connection.InvokeAsync("SendMessage", _conversation.Id, "Unauthorized message");

            // Assert - verify no message was saved
            var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var messageCount = await dbContext.Messages
                .CountAsync(m => m.SenderId == unauthorizedUser.Id);
            
            Assert.Equal(0, messageCount);
        }
        finally
        {
            await connection.StopAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task ChatHub_MarkAsRead_UnreadMessages_MarksThemAsRead()
    {
        // Arrange
        var message1 = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = _conversation.Id,
            SenderId = _user1.Id,
            Content = "Test message 1",
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        var message2 = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = _conversation.Id,
            SenderId = _user1.Id,
            Content = "Test message 2",
            SentAt = DateTime.UtcNow.AddMinutes(1),
            IsRead = false
        };

        _dbContext.Messages.AddRange(message1, message2);
        await _dbContext.SaveChangesAsync();

        var token = _jwtService.GenerateToken(_user2);
        var connection = CreateHubConnection(token);

        try
        {
            await connection.StartAsync();
            await connection.InvokeAsync("JoinConversation", _conversation.Id);

            // Act
            await connection.InvokeAsync("MarkAsRead", _conversation.Id);

            // Allow time for database update
            await Task.Delay(500);

            // Assert
            var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var messages = await dbContext.Messages
                .Where(m => m.ConversationId == _conversation.Id)
                .ToListAsync();

            Assert.All(messages, m => Assert.True(m.IsRead));
        }
        finally
        {
            await connection.StopAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task ChatHub_TypingIndicators_BroadcastsToOtherUsers()
    {
        // Arrange
        var token1 = _jwtService.GenerateToken(_user1);
        var token2 = _jwtService.GenerateToken(_user2);
        
        var connection1 = CreateHubConnection(token1);
        var connection2 = CreateHubConnection(token2);

        var typingReceived = false;
        var stoppedTypingReceived = false;
        var typingTcs = new TaskCompletionSource<bool>();
        var stoppedTypingTcs = new TaskCompletionSource<bool>();

        connection2.On<Guid, Guid>("UserTyping", (convId, userId) =>
        {
            if (convId == _conversation.Id && userId == _user1.Id)
            {
                typingReceived = true;
                typingTcs.TrySetResult(true);
            }
        });

        connection2.On<Guid, Guid>("UserStoppedTyping", (convId, userId) =>
        {
            if (convId == _conversation.Id && userId == _user1.Id)
            {
                stoppedTypingReceived = true;
                stoppedTypingTcs.TrySetResult(true);
            }
        });

        try
        {
            await connection1.StartAsync();
            await connection2.StartAsync();

            await connection1.InvokeAsync("JoinConversation", _conversation.Id);
            await connection2.InvokeAsync("JoinConversation", _conversation.Id);

            // Act
            await connection1.InvokeAsync("StartTyping", _conversation.Id);
            await Task.WhenAny(typingTcs.Task, Task.Delay(3000));

            await connection1.InvokeAsync("StopTyping", _conversation.Id);
            await Task.WhenAny(stoppedTypingTcs.Task, Task.Delay(3000));

            // Assert
            Assert.True(typingReceived, "Typing indicator was not received");
            Assert.True(stoppedTypingReceived, "Stopped typing indicator was not received");
        }
        finally
        {
            await connection1.StopAsync();
            await connection2.StopAsync();
            await connection1.DisposeAsync();
            await connection2.DisposeAsync();
        }
    }

    [Fact]
    public async Task ChatHub_Disconnect_RemovesUserFromConnections()
    {
        // Arrange
        var token = _jwtService.GenerateToken(_user1);
        var connection = CreateHubConnection(token);

        await connection.StartAsync();
        var connectionId = connection.ConnectionId;
        Assert.NotNull(connectionId);

        // Act
        await connection.StopAsync();
        await connection.DisposeAsync();

        // Assert
        // Connection should be properly cleaned up (no exception on dispose)
        Assert.Equal(HubConnectionState.Disconnected, connection.State);
    }

    private HubConnection CreateHubConnection(string? token)
    {
        var hubUrl = $"{_factory.Server.BaseAddress}hubs/chat";
        
        var builder = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                if (!string.IsNullOrEmpty(token))
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(token);
                }
            });

        return builder.Build();
    }
}

