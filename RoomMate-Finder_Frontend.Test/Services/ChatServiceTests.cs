using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Moq;
using RoomMate_Finder_Frontend.Services;
using RoomMate_Finder_Frontend.Services.Internals;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Services;

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

    [Fact]
    public async Task ConnectAsync_ShouldStartConnection_WhenNotConnected()
    {
        // Act
        await _service.ConnectAsync("token");

        // Assert
        _mockFactory.Verify(f => f.CreateConnection(_hubUrl, "token"), Times.Once);
        _mockHubConnection.Verify(c => c.StartAsync(default), Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_ShouldInvokeHubMethod_WhenConnected()
    {
        // Arrange
        await _service.ConnectAsync("token");
        var convId = Guid.NewGuid();
        var content = "test";

        // Act
        await _service.SendMessageAsync(convId, content);

        // Assert
        _mockHubConnection.Verify(c => c.InvokeAsync("SendMessage", convId, content, default), Times.Once);
    }

    [Fact]
    public async Task JoinConversationAsync_ShouldInvokeHubMethod_WhenConnected()
    {
        // Arrange
        await _service.ConnectAsync("token");
        var convId = Guid.NewGuid();

        // Act
        await _service.JoinConversationAsync(convId);

        // Assert
        _mockHubConnection.Verify(c => c.InvokeAsync("JoinConversation", convId, default), Times.Once);
    }
    
    [Fact]
    public void OnMessageReceived_ShouldFire_WhenHubReceivesMessage()
    {
        // Arrange
        Action<ChatMessageDto>? handler = null;
        _mockHubConnection.Setup(c => c.On("ReceiveMessage", It.IsAny<Action<ChatMessageDto>>()))
            .Callback<string, Action<ChatMessageDto>>((name, h) => handler = h)
            .Returns(Mock.Of<IDisposable>());

        // Act - Trigger Connect which registers handlers
        _service.ConnectAsync("token"); // Not awaiting to keep sync context if feasible, or Mock StartAsync returns completed task (default)

        // Verify handler registered
        handler.Should().NotBeNull();

        // Act - Trigger handler
        var msg = new ChatMessageDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Sender", "User", "Content", DateTime.UtcNow, false);
        bool eventFired = false;
        _service.OnMessageReceived += (m) => 
        {
            eventFired = true;
            m.Should().Be(msg); 
        };

        handler!.Invoke(msg);

        // Assert
        eventFired.Should().BeTrue();
    }
}
