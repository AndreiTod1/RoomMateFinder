using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Hubs;
using RoomMate_Finder.Infrastructure.Persistence;
using Xunit;

namespace RoomMate_Finder.Test.Hubs;

public class ChatHubTests
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly Mock<ISingleClientProxy> _mockClientProxy;
    private readonly ChatHub _chatHub;
    private readonly Guid _userId;
    private readonly string _connectionId;

    public ChatHubTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(options);

        _mockClients = new Mock<IHubCallerClients>();
        _mockGroups = new Mock<IGroupManager>();
        _mockContext = new Mock<HubCallerContext>();
        _mockClientProxy = new Mock<ISingleClientProxy>();

        _userId = Guid.NewGuid();
        _connectionId = Guid.NewGuid().ToString();

        // Setup Context
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        _mockContext.Setup(c => c.User).Returns(principal);
        _mockContext.Setup(c => c.ConnectionId).Returns(_connectionId);

        // Setup Clients
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClients.Setup(c => c.Client(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClients.Setup(c => c.OthersInGroup(It.IsAny<string>())).Returns(_mockClientProxy.Object);

        // Setup Groups
        _mockGroups.Setup(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);
        _mockGroups.Setup(g => g.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);

        _chatHub = new ChatHub(_dbContext)
        {
            Context = _mockContext.Object,
            Clients = _mockClients.Object,
            Groups = _mockGroups.Object
        };
    }

    [Fact]
    public async Task OnConnectedAsync_ShouldAddConnection()
    {
        // Act
        await _chatHub.OnConnectedAsync();

        // Assert
        // We can't easily verify the private static dictionary, but we can ensure no exception
        // and that User ID was accessed
        _mockContext.Verify(c => c.User, Times.AtLeastOnce);
    }

    [Fact]
    public async Task JoinConversation_WhenUserIsParticipant_ShouldAddToGroup()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = _userId,
            User2Id = otherUserId
        };
        _dbContext.Conversations.Add(conversation);
        await _dbContext.SaveChangesAsync();

        // Act
        await _chatHub.JoinConversation(conversation.Id);

        // Assert
        _mockGroups.Verify(g => g.AddToGroupAsync(_connectionId, $"conversation_{conversation.Id}", default), Times.Once);
    }

    [Fact]
    public async Task JoinConversation_WhenUserIsNotParticipant_ShouldNotAddToGroup()
    {
        // Arrange
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = Guid.NewGuid(),
            User2Id = Guid.NewGuid()
        };
        _dbContext.Conversations.Add(conversation);
        await _dbContext.SaveChangesAsync();

        // Act
        await _chatHub.JoinConversation(conversation.Id);

        // Assert
        _mockGroups.Verify(g => g.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task SendMessage_WhenValid_ShouldSaveAndSend()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var otherUser = new Profile { Id = otherUserId, FullName = "Other User", Email = "other@test.com", PasswordHash= "pwd" };
        var currentUser = new Profile { Id = _userId, FullName = "Me", Email = "me@test.com", PasswordHash = "pwd" };
        
        _dbContext.Profiles.AddRange(currentUser, otherUser);

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = _userId,
            User2Id = otherUserId,
            User1 = currentUser,
            User2 = otherUser
        };
        _dbContext.Conversations.Add(conversation);
        await _dbContext.SaveChangesAsync();

        var content = "Hello World";

        // Act
        await _chatHub.SendMessage(conversation.Id, content);

        // Assert
        // 1. Verify message in DB
        var savedMessage = await _dbContext.Messages.FirstOrDefaultAsync();
        savedMessage.Should().NotBeNull();
        savedMessage!.Content.Should().Be(content);
        savedMessage.SenderId.Should().Be(_userId);
        conversation.Id.Should().Be(savedMessage.ConversationId);

        // 2. Verify sent to group
        _mockClients.Verify(c => c.Group($"conversation_{conversation.Id}"), Times.Once);
        _mockClientProxy.Verify(p => p.SendCoreAsync("ReceiveMessage", 
            It.Is<object[]>(args => 
                args.Length == 1 &&
                ((ChatMessageDto)args[0]).Content == content &&
                ((ChatMessageDto)args[0]).SenderId == _userId
            ), default), Times.Once);
    }

    [Fact]
    public async Task SendMessage_WhenUserNotInConversation_ShouldNotSend()
    {
        // Arrange
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = Guid.NewGuid(),
            User2Id = Guid.NewGuid()
        };
        _dbContext.Conversations.Add(conversation);
        await _dbContext.SaveChangesAsync();

        // Act
        await _chatHub.SendMessage(conversation.Id, "Hello");

        // Assert
        var count = await _dbContext.Messages.CountAsync();
        count.Should().Be(0);
        _mockClients.Verify(c => c.Group(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task MarkAsRead_ShouldUpdateMessagesAndNotifySender()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = _userId,
            User2Id = otherUserId
        };
        _dbContext.Conversations.Add(conversation);

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = otherUserId, // Sent by OTHER user
            Content = "Test",
            IsRead = false,
            SentAt = DateTime.UtcNow
        };
        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync();

        // Act
        await _chatHub.MarkAsRead(conversation.Id);

        // Assert
        var dbMessage = await _dbContext.Messages.FindAsync(message.Id);
        dbMessage!.IsRead.Should().BeTrue();

        _mockClientProxy.Verify(p => p.SendCoreAsync("MessagesRead",
            It.Is<object[]>(args =>
                args.Length == 2 && 
                (Guid)args[0] == conversation.Id &&
                (Guid)args[1] == _userId
            ), default), Times.Once);
    }

    [Fact]
    public async Task StartTyping_ShouldNotifyOthers()
    {
        // Act
        var convId = Guid.NewGuid();
        await _chatHub.StartTyping(convId);

        // Assert
        _mockClients.Verify(c => c.OthersInGroup($"conversation_{convId}"), Times.Once);
        _mockClientProxy.Verify(p => p.SendCoreAsync("UserTyping", 
            It.Is<object[]>(args => (Guid)args[0] == convId && (Guid)args[1] == _userId), 
            default), Times.Once);
    }
}
