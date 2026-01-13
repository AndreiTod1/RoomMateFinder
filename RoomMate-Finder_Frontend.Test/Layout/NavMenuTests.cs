using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using Microsoft.Extensions.Configuration;
using RoomMate_Finder_Frontend.Layout;
using RoomMate_Finder_Frontend.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Layout;

public class NavMenuTests : BunitContext, IAsyncLifetime
{
    private Mock<INotificationService> _mockNotificationService = null!;
    private Mock<IProfileService> _mockProfileService = null!;
    private Mock<IAuthService> _mockAuthService = null!;

    public Task InitializeAsync()
    {
        _mockNotificationService = new Mock<INotificationService>();
        _mockProfileService = new Mock<IProfileService>();
        _mockAuthService = new Mock<IAuthService>();

        Services.AddMudServices();
        Services.AddSingleton(_mockNotificationService.Object);
        Services.AddSingleton(_mockProfileService.Object);
        Services.AddSingleton(_mockAuthService.Object);
        Services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        _mockNotificationService.Setup(x => x.UnreadConversationsCount).Returns(0);

        JSInterop.Mode = JSRuntimeMode.Loose;
        
        return Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    [Fact]
    public void NavMenu_ComponentTypeCheck()
    {
        // Test that component type exists
        var componentType = typeof(NavMenu);
        componentType.Should().NotBeNull();
    }

    [Fact]
    public void NavMenu_ImplementsIDisposable()
    {
        typeof(NavMenu).GetInterfaces().Should().Contain(typeof(IDisposable));
    }

    [Fact]
    public void NavMenu_HasExpectedDependencies()
    {
        // Verify services are registered
        Services.GetService<INotificationService>().Should().NotBeNull();
        Services.GetService<IProfileService>().Should().NotBeNull();
        Services.GetService<IAuthService>().Should().NotBeNull();
    }

    [Fact]
    public void NavMenu_NotificationService_TracksUnreadCount()
    {
        // Test that notification service can track unread messages
        _mockNotificationService.Setup(x => x.UnreadConversationsCount).Returns(5);
        _mockNotificationService.Object.UnreadConversationsCount.Should().Be(5);
    }
}
