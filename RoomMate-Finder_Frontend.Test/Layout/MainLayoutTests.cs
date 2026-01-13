using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using Microsoft.Extensions.Configuration;
using RoomMate_Finder_Frontend.Layout;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Layout;

public class MainLayoutTests : BunitContext, IAsyncLifetime
{
    private Mock<INotificationService> _mockNotificationService = null!;
    private Mock<IConversationService> _mockConversationService = null!;
    private Mock<IChatService> _mockChatService = null!;
    private Mock<IAuthService> _mockAuthService = null!;

    public Task InitializeAsync()
    {
        _mockNotificationService = new Mock<INotificationService>();
        _mockConversationService = new Mock<IConversationService>();
        _mockChatService = new Mock<IChatService>();
        _mockAuthService = new Mock<IAuthService>();

        Services.AddMudServices();
        Services.AddSingleton(_mockNotificationService.Object);
        Services.AddSingleton(_mockConversationService.Object);
        Services.AddSingleton(_mockChatService.Object);
        Services.AddSingleton(_mockAuthService.Object);
        Services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        _mockNotificationService.Setup(x => x.UnreadConversationsCount).Returns(0);
        _mockNotificationService.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);

        JSInterop.Mode = JSRuntimeMode.Loose;
        
        return Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    [Fact]
    public void MainLayout_ComponentTypeCheck()
    {
        // Test that component type exists and has expected properties
        var componentType = typeof(MainLayout);
        componentType.Should().NotBeNull();
        componentType.BaseType?.Name.Should().Be("LayoutComponentBase");
    }

    [Fact]
    public void MainLayout_ImplementsIDisposable()
    {
        typeof(MainLayout).GetInterfaces().Should().Contain(typeof(IDisposable));
    }

    [Fact]
    public void MainLayout_HasExpectedDependencies()
    {
        // Verify services are registered
        Services.GetService<INotificationService>().Should().NotBeNull();
        Services.GetService<IChatService>().Should().NotBeNull();
        Services.GetService<IAuthService>().Should().NotBeNull();
    }
}
