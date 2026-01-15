using FluentAssertions;
using Microsoft.JSInterop;
using Moq;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Services;

public class NotificationServiceTests
{
    private Mock<IJSRuntime> CreateMockJsRuntime(string? storedValue = null)
    {
        var mock = new Mock<IJSRuntime>();
        mock.Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(storedValue);
        // InvokeVoidAsync is an extension method, so we mock InvokeAsync<object> with null return
        mock.Setup(x => x.InvokeAsync<object>("localStorage.setItem", It.IsAny<object[]>()))
            .ReturnsAsync((object?)null);
        return mock;
    }

    #region InitializeAsync Tests

    [Fact]
    public async Task Given_EmptyStorage_When_InitializeAsyncIsCalled_Then_InitializesEmpty()
    {
        // Arrange
        var jsMock = CreateMockJsRuntime(null);
        var service = new NotificationService(jsMock.Object);

        // Act
        await service.InitializeAsync();

        // Assert
        service.UnreadConversationsCount.Should().Be(0);
    }

    [Fact]
    public async Task Given_StoredIds_When_InitializeAsyncIsCalled_Then_LoadsFromStorage()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var jsMock = CreateMockJsRuntime($"{id1},{id2}");
        var service = new NotificationService(jsMock.Object);

        // Act
        await service.InitializeAsync();

        // Assert
        service.UnreadConversationsCount.Should().Be(2);
        service.HasUnreadMessages(id1).Should().BeTrue();
        service.HasUnreadMessages(id2).Should().BeTrue();
    }

    [Fact]
    public async Task Given_InvalidGuids_When_InitializeAsyncIsCalled_Then_IgnoresInvalidIds()
    {
        // Arrange
        var validId = Guid.NewGuid();
        var jsMock = CreateMockJsRuntime($"{validId},invalid-guid,another-bad");
        var service = new NotificationService(jsMock.Object);

        // Act
        await service.InitializeAsync();

        // Assert
        service.UnreadConversationsCount.Should().Be(1);
        service.HasUnreadMessages(validId).Should().BeTrue();
    }

    [Fact]
    public async Task Given_AlreadyInitialized_When_InitializeAsyncIsCalled_Then_DoesNotReinitialize()
    {
        // Arrange
        var id = Guid.NewGuid();
        var jsMock = CreateMockJsRuntime($"{id}");
        var service = new NotificationService(jsMock.Object);

        await service.InitializeAsync();
        await service.ClearAllAsync();

        // Act
        await service.InitializeAsync();

        // Assert - should still be empty because clear was called and init is skipped
        service.UnreadConversationsCount.Should().Be(0);
    }

    [Fact]
    public async Task Given_JsException_When_InitializeAsyncIsCalled_Then_HandlesGracefully()
    {
        // Arrange
        var jsMock = new Mock<IJSRuntime>();
        jsMock.Setup(x => x.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ThrowsAsync(new JSException("Test error"));
        var service = new NotificationService(jsMock.Object);

        // Act
        await service.InitializeAsync();

        // Assert - should not throw and be initialized
        service.UnreadConversationsCount.Should().Be(0);
    }

    #endregion

    #region SyncFromServerAsync Tests

    [Fact]
    public async Task Given_ServerData_When_SyncFromServerAsyncIsCalled_Then_ReplacesLocalData()
    {
        // Arrange
        var jsMock = CreateMockJsRuntime(null);
        var service = new NotificationService(jsMock.Object);
        var serverIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        await service.SyncFromServerAsync(serverIds);

        // Assert
        service.UnreadConversationsCount.Should().Be(2);
    }

    [Fact]
    public async Task Given_SyncFromServer_When_Called_Then_TriggersNotificationChangedEvent()
    {
        // Arrange
        var jsMock = CreateMockJsRuntime(null);
        var service = new NotificationService(jsMock.Object);
        var eventTriggered = false;
        service.OnNotificationsChanged += () => eventTriggered = true;

        // Act
        await service.SyncFromServerAsync(new List<Guid> { Guid.NewGuid() });

        // Assert
        eventTriggered.Should().BeTrue();
    }

    #endregion

    #region AddUnreadConversationAsync Tests

    [Fact]
    public async Task Given_NewConversation_When_AddUnreadConversationAsyncIsCalled_Then_AddsToList()
    {
        // Arrange
        var jsMock = CreateMockJsRuntime(null);
        var service = new NotificationService(jsMock.Object);
        var conversationId = Guid.NewGuid();

        // Act
        await service.AddUnreadConversationAsync(conversationId);

        // Assert
        service.UnreadConversationsCount.Should().Be(1);
        service.HasUnreadMessages(conversationId).Should().BeTrue();
    }

    [Fact]
    public async Task Given_ExistingConversation_When_AddUnreadConversationAsyncIsCalled_Then_DoesNotDuplicate()
    {
        // Arrange
        var jsMock = CreateMockJsRuntime(null);
        var service = new NotificationService(jsMock.Object);
        var conversationId = Guid.NewGuid();

        await service.AddUnreadConversationAsync(conversationId);

        // Act
        await service.AddUnreadConversationAsync(conversationId);

        // Assert
        service.UnreadConversationsCount.Should().Be(1);
    }

    [Fact]
    public async Task Given_Add_When_Called_Then_TriggersNotificationChangedEvent()
    {
        // Arrange
        var jsMock = CreateMockJsRuntime(null);
        var service = new NotificationService(jsMock.Object);
        var eventTriggered = false;
        service.OnNotificationsChanged += () => eventTriggered = true;

        // Act
        await service.AddUnreadConversationAsync(Guid.NewGuid());

        // Assert
        eventTriggered.Should().BeTrue();
    }

    #endregion

    #region MarkConversationAsReadAsync Tests

    [Fact]
    public async Task Given_ExistingUnread_When_MarkConversationAsReadAsyncIsCalled_Then_RemovesFromList()
    {
        // Arrange
        var jsMock = CreateMockJsRuntime(null);
        var service = new NotificationService(jsMock.Object);
        var conversationId = Guid.NewGuid();
        await service.AddUnreadConversationAsync(conversationId);

        // Act
        await service.MarkConversationAsReadAsync(conversationId);

        // Assert
        service.UnreadConversationsCount.Should().Be(0);
        service.HasUnreadMessages(conversationId).Should().BeFalse();
    }

    [Fact]
    public async Task Given_NonExistingConversation_When_MarkConversationAsReadAsyncIsCalled_Then_DoesNothing()
    {
        // Arrange
        var jsMock = CreateMockJsRuntime(null);
        var service = new NotificationService(jsMock.Object);

        // Act
        await service.MarkConversationAsReadAsync(Guid.NewGuid());

        // Assert
        service.UnreadConversationsCount.Should().Be(0);
    }

    [Fact]
    public async Task Given_MarkAsRead_When_Called_Then_TriggersNotificationChangedEvent()
    {
        // Arrange
        var jsMock = CreateMockJsRuntime(null);
        var service = new NotificationService(jsMock.Object);
        var conversationId = Guid.NewGuid();
        await service.AddUnreadConversationAsync(conversationId);
        
        var eventTriggered = false;
        service.OnNotificationsChanged += () => eventTriggered = true;

        // Act
        await service.MarkConversationAsReadAsync(conversationId);

        // Assert
        eventTriggered.Should().BeTrue();
    }

    #endregion

    #region ClearAllAsync Tests

    [Fact]
    public async Task Given_MultipleUnread_When_ClearAllAsyncIsCalled_Then_ClearsAll()
    {
        // Arrange
        var jsMock = CreateMockJsRuntime(null);
        var service = new NotificationService(jsMock.Object);
        await service.AddUnreadConversationAsync(Guid.NewGuid());
        await service.AddUnreadConversationAsync(Guid.NewGuid());

        // Act
        await service.ClearAllAsync();

        // Assert
        service.UnreadConversationsCount.Should().Be(0);
    }

    [Fact]
    public async Task Given_ClearAll_When_Called_Then_TriggersNotificationChangedEvent()
    {
        // Arrange
        var jsMock = CreateMockJsRuntime(null);
        var service = new NotificationService(jsMock.Object);
        await service.AddUnreadConversationAsync(Guid.NewGuid());

        var eventTriggered = false;
        service.OnNotificationsChanged += () => eventTriggered = true;

        // Act
        await service.ClearAllAsync();

        // Assert
        eventTriggered.Should().BeTrue();
    }

    #endregion

    #region HasUnreadMessages Tests

    [Fact]
    public async Task Given_UnreadConversation_When_HasUnreadMessagesIsCalled_Then_ReturnsTrue()
    {
        // Arrange
        var jsMock = CreateMockJsRuntime(null);
        var service = new NotificationService(jsMock.Object);
        var conversationId = Guid.NewGuid();
        await service.AddUnreadConversationAsync(conversationId);

        // Act
        var result = service.HasUnreadMessages(conversationId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Given_NoUnreadConversation_When_HasUnreadMessagesIsCalled_Then_ReturnsFalse()
    {
        // Arrange
        var jsMock = CreateMockJsRuntime(null);
        var service = new NotificationService(jsMock.Object);

        // Act
        var result = service.HasUnreadMessages(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UnreadConversations Property Tests

    [Fact]
    public async Task Given_AddedConversations_When_UnreadConversationsAccessed_Then_ReturnsSet()
    {
        // Arrange
        var jsMock = CreateMockJsRuntime(null);
        var service = new NotificationService(jsMock.Object);
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        await service.AddUnreadConversationAsync(id1);
        await service.AddUnreadConversationAsync(id2);

        // Act
        var result = service.UnreadConversations;

        // Assert
        result.Should().Contain(id1);
        result.Should().Contain(id2);
    }

    #endregion
}
