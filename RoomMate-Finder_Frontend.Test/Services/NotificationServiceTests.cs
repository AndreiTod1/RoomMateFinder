using FluentAssertions;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Services;

public class NotificationServiceTests
{
    #region INotificationService Interface Contract Tests

    [Fact]
    public void INotificationService_Interface_ShouldExist()
    {
        // Assert
        typeof(INotificationService).Should().NotBeNull();
        typeof(INotificationService).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void INotificationService_Should_HaveInitializeAsyncMethod()
    {
        // Assert
        var method = typeof(INotificationService).GetMethod("InitializeAsync");
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(Task));
    }

    [Fact]
    public void INotificationService_Should_HaveSyncFromServerAsyncMethod()
    {
        // Assert
        var method = typeof(INotificationService).GetMethod("SyncFromServerAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void INotificationService_Should_HaveAddUnreadConversationAsyncMethod()
    {
        // Assert
        var method = typeof(INotificationService).GetMethod("AddUnreadConversationAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void INotificationService_Should_HaveMarkConversationAsReadAsyncMethod()
    {
        // Assert
        var method = typeof(INotificationService).GetMethod("MarkConversationAsReadAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void INotificationService_Should_HaveClearAllAsyncMethod()
    {
        // Assert
        var method = typeof(INotificationService).GetMethod("ClearAllAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void INotificationService_Should_HaveHasUnreadMessagesMethod()
    {
        // Assert
        var method = typeof(INotificationService).GetMethod("HasUnreadMessages");
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be(typeof(bool));
    }

    [Fact]
    public void INotificationService_Should_HaveUnreadConversationsCountProperty()
    {
        // Assert
        var property = typeof(INotificationService).GetProperty("UnreadConversationsCount");
        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(int));
    }

    [Fact]
    public void INotificationService_Should_HaveUnreadConversationsProperty()
    {
        // Assert
        var property = typeof(INotificationService).GetProperty("UnreadConversations");
        property.Should().NotBeNull();
        property!.PropertyType.Should().Be(typeof(HashSet<Guid>));
    }

    [Fact]
    public void INotificationService_Should_HaveOnNotificationsChangedEvent()
    {
        // Assert
        var eventInfo = typeof(INotificationService).GetEvent("OnNotificationsChanged");
        eventInfo.Should().NotBeNull();
    }

    #endregion

    #region NotificationService Implementation Tests

    [Fact]
    public void NotificationService_Should_ImplementINotificationService()
    {
        // Assert
        typeof(NotificationService).Should().Implement<INotificationService>();
    }

    #endregion
}

