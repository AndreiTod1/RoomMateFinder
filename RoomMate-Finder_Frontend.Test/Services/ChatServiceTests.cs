using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Moq;
using RoomMate_Finder_Frontend.Services;
using RoomMate_Finder_Frontend.Services.Internals;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Services;

/// <summary>
/// Comprehensive tests for ChatService covering all code paths.
/// Tests all methods: Connect, Disconnect, Join, Leave, Send, MarkAsRead, Typing, Events.
/// </summary>
public class ChatServiceTests
{
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly Mock<IHubConnectionFactory> _mockFactory;
    private readonly Mock<IHubConnection> _mockHubConnection;
    private readonly ChatService _service;
    private readonly string _hubUrl = "http://localhost/hubs/chat";

    public ChatServiceTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockConfig.Setup(c => c["ApiBaseUrl"]).Returns("http://localhost");

        _mockHubConnection = new Mock<IHubConnection>();
        _mockHubConnection.Setup(c => c.State).Returns(HubConnectionState.Connected);
        
        _mockFactory = new Mock<IHubConnectionFactory>();
        _mockFactory.Setup(f => f.CreateConnection(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(_mockHubConnection.Object);

        _service = new ChatService(_mockConfig.Object, _mockFactory.Object);
    }

    #region Interface Tests

    [Fact]
    public void ChatService_ImplementsIChatService()
    {
        typeof(ChatService).GetInterfaces().Should().Contain(typeof(IChatService));
    }

    [Fact]
    public void ChatService_ImplementsIAsyncDisposable()
    {
        typeof(ChatService).GetInterfaces().Should().Contain(typeof(IAsyncDisposable));
    }

    [Fact]
    public void IChatService_HasOnMessageReceivedEvent()
    {
        var eventInfo = typeof(IChatService).GetEvent("OnMessageReceived");
        eventInfo.Should().NotBeNull();
    }

    [Fact]
    public void IChatService_HasOnNewMessageNotificationEvent()
    {
        var eventInfo = typeof(IChatService).GetEvent("OnNewMessageNotification");
        eventInfo.Should().NotBeNull();
    }

    [Fact]
    public void IChatService_HasOnMessagesReadEvent()
    {
        var eventInfo = typeof(IChatService).GetEvent("OnMessagesRead");
        eventInfo.Should().NotBeNull();
    }

    [Fact]
    public void IChatService_HasOnUserTypingEvent()
    {
        var eventInfo = typeof(IChatService).GetEvent("OnUserTyping");
        eventInfo.Should().NotBeNull();
    }

    [Fact]
    public void IChatService_HasOnUserStoppedTypingEvent()
    {
        var eventInfo = typeof(IChatService).GetEvent("OnUserStoppedTyping");
        eventInfo.Should().NotBeNull();
    }

    [Fact]
    public void IChatService_HasIsConnectedProperty()
    {
        var property = typeof(IChatService).GetProperty("IsConnected");
        property.Should().NotBeNull();
    }

    #endregion

    #region Connection Tests

    [Fact]
    public async Task ConnectAsync_ShouldStartConnection()
    {
        await _service.ConnectAsync("token");

        _mockFactory.Verify(f => f.CreateConnection(_hubUrl, "token"), Times.Once);
        _mockHubConnection.Verify(c => c.StartAsync(default), Times.Once);
    }

    [Fact]
    public async Task ConnectAsync_WhenAlreadyConnected_DisconnectsFirst()
    {
        // First connection
        await _service.ConnectAsync("token1");
        
        // Second connection should disconnect first
        await _service.ConnectAsync("token2");
        
        _mockHubConnection.Verify(c => c.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task DisconnectAsync_ShouldDisposeConnection()
    {
        await _service.ConnectAsync("token");
        await _service.DisconnectAsync();

        _mockHubConnection.Verify(c => c.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task DisconnectAsync_WhenNotConnected_DoesNothing()
    {
        // Should not throw
        await _service.DisconnectAsync();
        
        _mockHubConnection.Verify(c => c.DisposeAsync(), Times.Never);
    }

    [Fact]
    public void IsConnected_ReturnsFalse_WhenNotConnected()
    {
        _service.IsConnected.Should().BeFalse();
    }

    [Fact]
    public async Task IsConnected_ReturnsTrue_WhenConnected()
    {
        await _service.ConnectAsync("token");
        _service.IsConnected.Should().BeTrue();
    }

    #endregion

    #region Message Methods Tests

    [Fact]
    public async Task SendMessageAsync_ShouldInvokeHubMethod()
    {
        await _service.ConnectAsync("token");
        var convId = Guid.NewGuid();
        var content = "test message";

        await _service.SendMessageAsync(convId, content);

        _mockHubConnection.Verify(c => c.InvokeAsync("SendMessage", convId, content, default), Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_WhenNotConnected_DoesNothing()
    {
        // Arrange - setup disconnected state
        _mockHubConnection.Setup(c => c.State).Returns(HubConnectionState.Disconnected);
        await _service.ConnectAsync("token");
        
        var convId = Guid.NewGuid();
        await _service.SendMessageAsync(convId, "test");

        // Should not invoke when disconnected
        _mockHubConnection.Verify(c => c.InvokeAsync("SendMessage", It.IsAny<Guid>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task JoinConversationAsync_ShouldInvokeHubMethod()
    {
        await _service.ConnectAsync("token");
        var convId = Guid.NewGuid();

        await _service.JoinConversationAsync(convId);

        _mockHubConnection.Verify(c => c.InvokeAsync("JoinConversation", convId, default), Times.Once);
    }

    [Fact]
    public async Task LeaveConversationAsync_ShouldInvokeHubMethod()
    {
        await _service.ConnectAsync("token");
        var convId = Guid.NewGuid();

        await _service.LeaveConversationAsync(convId);

        _mockHubConnection.Verify(c => c.InvokeAsync("LeaveConversation", convId, default), Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_ShouldInvokeHubMethod()
    {
        await _service.ConnectAsync("token");
        var convId = Guid.NewGuid();

        await _service.MarkAsReadAsync(convId);

        _mockHubConnection.Verify(c => c.InvokeAsync("MarkAsRead", convId, default), Times.Once);
    }

    #endregion

    #region Typing Methods Tests

    [Fact]
    public async Task StartTypingAsync_ShouldInvokeHubMethod()
    {
        await _service.ConnectAsync("token");
        var convId = Guid.NewGuid();

        await _service.StartTypingAsync(convId);

        _mockHubConnection.Verify(c => c.InvokeAsync("StartTyping", convId, default), Times.Once);
    }

    [Fact]
    public async Task StopTypingAsync_ShouldInvokeHubMethod()
    {
        await _service.ConnectAsync("token");
        var convId = Guid.NewGuid();

        await _service.StopTypingAsync(convId);

        _mockHubConnection.Verify(c => c.InvokeAsync("StopTyping", convId, default), Times.Once);
    }

    #endregion

    #region Event Handler Tests

    [Fact]
    public void OnMessageReceived_ShouldFire_WhenHubReceivesMessage()
    {
        Action<ChatMessageDto>? handler = null;
        _mockHubConnection.Setup(c => c.On("ReceiveMessage", It.IsAny<Action<ChatMessageDto>>()))
            .Callback<string, Action<ChatMessageDto>>((name, h) => handler = h)
            .Returns(Mock.Of<IDisposable>());

        _service.ConnectAsync("token");

        handler.Should().NotBeNull();

        var msg = new ChatMessageDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Sender", "User", "Content", DateTime.UtcNow, false);
        bool eventFired = false;
        _service.OnMessageReceived += (m) => 
        {
            eventFired = true;
            m.Should().Be(msg); 
        };

        handler!.Invoke(msg);

        eventFired.Should().BeTrue();
    }

    [Fact]
    public void OnNewMessageNotification_ShouldFire_WhenHubReceivesNotification()
    {
        Action<Guid, string>? handler = null;
        _mockHubConnection.Setup(c => c.On("NewMessageNotification", It.IsAny<Action<Guid, string>>()))
            .Callback<string, Action<Guid, string>>((name, h) => handler = h)
            .Returns(Mock.Of<IDisposable>());

        _service.ConnectAsync("token");

        handler.Should().NotBeNull();

        var convId = Guid.NewGuid();
        var senderName = "Test User";
        bool eventFired = false;
        _service.OnNewMessageNotification += (cId, name) => 
        {
            eventFired = true;
            cId.Should().Be(convId);
            name.Should().Be(senderName);
        };

        handler!.Invoke(convId, senderName);

        eventFired.Should().BeTrue();
    }

    [Fact]
    public void OnMessagesRead_ShouldFire_WhenHubReceivesRead()
    {
        Action<Guid, Guid>? handler = null;
        _mockHubConnection.Setup(c => c.On("MessagesRead", It.IsAny<Action<Guid, Guid>>()))
            .Callback<string, Action<Guid, Guid>>((name, h) => handler = h)
            .Returns(Mock.Of<IDisposable>());

        _service.ConnectAsync("token");

        handler.Should().NotBeNull();

        var convId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        bool eventFired = false;
        _service.OnMessagesRead += (cId, uId) => 
        {
            eventFired = true;
            cId.Should().Be(convId);
            uId.Should().Be(userId);
        };

        handler!.Invoke(convId, userId);

        eventFired.Should().BeTrue();
    }

    [Fact]
    public void OnUserTyping_ShouldFire_WhenHubReceivesTyping()
    {
        Action<Guid, Guid>? handler = null;
        _mockHubConnection.Setup(c => c.On("UserTyping", It.IsAny<Action<Guid, Guid>>()))
            .Callback<string, Action<Guid, Guid>>((name, h) => handler = h)
            .Returns(Mock.Of<IDisposable>());

        _service.ConnectAsync("token");

        handler.Should().NotBeNull();

        var convId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        bool eventFired = false;
        _service.OnUserTyping += (cId, uId) => 
        {
            eventFired = true;
            cId.Should().Be(convId);
            uId.Should().Be(userId);
        };

        handler!.Invoke(convId, userId);

        eventFired.Should().BeTrue();
    }

    [Fact]
    public void OnUserStoppedTyping_ShouldFire_WhenHubReceivesStoppedTyping()
    {
        Action<Guid, Guid>? handler = null;
        _mockHubConnection.Setup(c => c.On("UserStoppedTyping", It.IsAny<Action<Guid, Guid>>()))
            .Callback<string, Action<Guid, Guid>>((name, h) => handler = h)
            .Returns(Mock.Of<IDisposable>());

        _service.ConnectAsync("token");

        handler.Should().NotBeNull();

        var convId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        bool eventFired = false;
        _service.OnUserStoppedTyping += (cId, uId) => 
        {
            eventFired = true;
            cId.Should().Be(convId);
            uId.Should().Be(userId);
        };

        handler!.Invoke(convId, userId);

        eventFired.Should().BeTrue();
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public async Task DisposeAsync_ShouldDisconnect()
    {
        await _service.ConnectAsync("token");
        await _service.DisposeAsync();

        _mockHubConnection.Verify(c => c.DisposeAsync(), Times.Once);
    }

    #endregion

    #region ChatMessageDto Tests

    [Fact]
    public void ChatMessageDto_CanBeCreated()
    {
        var dto = new ChatMessageDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Sender",
            "User",
            "Content",
            DateTime.UtcNow,
            false
        );

        dto.Should().NotBeNull();
        dto.SenderName.Should().Be("Sender");
        dto.Content.Should().Be("Content");
        dto.IsRead.Should().BeFalse();
    }

    [Fact]
    public void ChatMessageDto_HasAllProperties()
    {
        typeof(ChatMessageDto).GetProperty("Id").Should().NotBeNull();
        typeof(ChatMessageDto).GetProperty("ConversationId").Should().NotBeNull();
        typeof(ChatMessageDto).GetProperty("SenderId").Should().NotBeNull();
        typeof(ChatMessageDto).GetProperty("SenderName").Should().NotBeNull();
        typeof(ChatMessageDto).GetProperty("SenderRole").Should().NotBeNull();
        typeof(ChatMessageDto).GetProperty("Content").Should().NotBeNull();
        typeof(ChatMessageDto).GetProperty("SentAt").Should().NotBeNull();
        typeof(ChatMessageDto).GetProperty("IsRead").Should().NotBeNull();
    }

    #endregion
}
