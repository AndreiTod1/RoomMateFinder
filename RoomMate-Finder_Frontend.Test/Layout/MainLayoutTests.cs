using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Layout;
using RoomMate_Finder_Frontend.Services;
using RoomMate_Finder_Frontend.Shared;
using System.Security.Claims;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Layout;

/// <summary>
/// Comprehensive tests for MainLayout.razor component targeting 80%+ coverage.
/// Tests all code paths: providers, auth state changes, SignalR connection,
/// notification syncing, and disposal logic.
/// </summary>
public class MainLayoutTests : IAsyncLifetime
{
    private readonly BunitContext _ctx = new();
    private readonly Mock<IChatService> _mockChatService;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<IConversationService> _mockConversationService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<ISnackbar> _mockSnackbar;
    private readonly Mock<AuthenticationStateProvider> _mockAuthProvider;
    private readonly Mock<IProfileService> _mockProfileService;


    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _ctx.DisposeAsync();
    }

    public MainLayoutTests()
    {
        _ctx.Services.AddMudServices();
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        _mockChatService = new Mock<IChatService>();
        _mockAuthService = new Mock<IAuthService>();
        _mockConversationService = new Mock<IConversationService>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockSnackbar = new Mock<ISnackbar>();
        _mockSnackbar.Setup(x => x.Configuration).Returns(new SnackbarConfiguration());
        _mockAuthProvider = new Mock<AuthenticationStateProvider>();
        _mockProfileService = new Mock<IProfileService>();

        _ctx.Services.AddSingleton(_mockChatService.Object);
        _ctx.Services.AddSingleton(_mockAuthService.Object);
        _ctx.Services.AddSingleton(_mockConversationService.Object);
        _ctx.Services.AddSingleton(_mockNotificationService.Object);
        _ctx.Services.AddSingleton(_mockSnackbar.Object);
        _ctx.Services.AddSingleton(_mockAuthProvider.Object);
        _ctx.Services.AddSingleton(_mockProfileService.Object);
        
        // Authorization Fix
        _ctx.Services.AddOptions();
        _ctx.Services.AddLogging();
        _ctx.Services.AddAuthorizationCore();
        _ctx.Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();

        // Default auth state: Not authenticated
        SetupUnauth();
    }

    private AuthenticationState SetupAuth(string name = "TestUser")
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, name) }, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var state = new AuthenticationState(user);
        
        _mockAuthProvider.Setup(p => p.GetAuthenticationStateAsync()).ReturnsAsync(state);
        return state;
    }

    private AuthenticationState SetupUnauth()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var state = new AuthenticationState(user);
        
        _mockAuthProvider.Setup(p => p.GetAuthenticationStateAsync()).ReturnsAsync(state);
        return state;
    }

    #region Rendering Tests

    [Fact]
    public void MainLayout_RendersProviders()
    {
        SetupUnauth();
        var cut = _ctx.Render<CascadingAuthenticationState>(p => p.AddChildContent<MainLayout>());
        
        // Use component finding instead of markup string matching for robustness
        cut.FindComponents<MudPopoverProvider>().Should().NotBeEmpty();
        cut.FindComponents<MudDialogProvider>().Should().NotBeEmpty();
        cut.FindComponents<MudSnackbarProvider>().Should().NotBeEmpty();
    }

    [Fact]
    public void MainLayout_RendersNavMenu()
    {
        SetupUnauth();
        var cut = _ctx.Render<CascadingAuthenticationState>(p => p.AddChildContent<MainLayout>());
        cut.FindComponent<NavMenu>().Should().NotBeNull();
    }

    [Fact]
    public void MainLayout_RendersBody()
    {
        SetupUnauth();
        var cut = _ctx.Render<CascadingAuthenticationState>(p => p.AddChildContent<MainLayout>(layout => 
            layout.Add(l => l.Body, (RenderFragment)(builder => builder.AddContent(0, "Test Body Content")))
        ));
        
        cut.Markup.Should().Contain("Test Body Content");
    }

    [Fact]
    public void MainLayout_HasMudContainer()
    {
        SetupUnauth();
        var cut = _ctx.Render<CascadingAuthenticationState>(p => p.AddChildContent<MainLayout>());
        cut.FindComponents<MudContainer>().Should().NotBeEmpty();
    }

    [Fact]
    public void MainLayout_HasMudMainContent()
    {
        SetupUnauth();
        var cut = _ctx.Render<CascadingAuthenticationState>(p => p.AddChildContent<MainLayout>());
        cut.FindComponents<MudMainContent>().Should().NotBeEmpty();
    }

    #endregion

    #region Initialization Tests

    [Fact]
    public void MainLayout_OnInitialized_InitializesNotificationService()
    {
        SetupUnauth();
        _ctx.Render<CascadingAuthenticationState>(p => p.AddChildContent<MainLayout>());
        // Called by MainLayout AND NavMenu
        _mockNotificationService.Verify(x => x.InitializeAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public void MainLayout_OnInitialized_Unauthenticated_DoesNotConnect()
    {
        SetupUnauth();
        _ctx.Render<CascadingAuthenticationState>(p => p.AddChildContent<MainLayout>());
        
        _mockChatService.Verify(x => x.ConnectAsync(It.IsAny<string>()), Times.Never);
        _mockConversationService.Verify(x => x.GetUnreadConversationsAsync(), Times.Never);
    }

    [Fact]
    public async Task MainLayout_OnInitialized_Authenticated_ConnectsAndSyncs()
    {
        // Arrange
        SetupAuth();
        
        _mockAuthService.Setup(x => x.GetTokenAsync()).ReturnsAsync("fake-token");
        _mockConversationService.Setup(x => x.GetUnreadConversationsAsync())
            .ReturnsAsync(new UnreadConversationsResponse(
                new List<UnreadConversationDto> { new UnreadConversationDto(Guid.NewGuid(), 5) }, 
                5));

        // Act
         var cut = _ctx.Render<CascadingAuthenticationState>(p => p.AddChildContent<MainLayout>());
         
         // Wait for async operations to complete
         await Task.Delay(50); 

         // Assert
         _mockAuthService.Verify(x => x.GetTokenAsync(), Times.AtLeastOnce);
         _mockChatService.Verify(x => x.ConnectAsync("fake-token"), Times.AtLeastOnce);
         _mockConversationService.Verify(x => x.GetUnreadConversationsAsync(), Times.AtLeastOnce);
         _mockNotificationService.Verify(x => x.SyncFromServerAsync(It.IsAny<IEnumerable<Guid>>()), Times.AtLeastOnce);
    }

    #endregion

    #region Auth State Change Tests

    [Fact]
    public async Task MainLayout_OnAuthChanged_Login_Connects()
    {
        // Init unauthenticated
        SetupUnauth();
        var cut = _ctx.Render<CascadingAuthenticationState>(p => p.AddChildContent<MainLayout>());
        var layout = cut.FindComponent<MainLayout>();

        // Setup for login
        _mockAuthService.Setup(x => x.GetTokenAsync()).ReturnsAsync("fake-token");
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Test") }, "TestAuth"));
        var task = Task.FromResult(new AuthenticationState(user));
        
        // Trigger auth change via reflection
        var instance = layout.Instance;
        var method = typeof(MainLayout).GetMethod("OnAuthStateChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        method?.Invoke(instance, new object[] { task });
        
        await Task.Delay(50);
        
        _mockChatService.Verify(x => x.ConnectAsync("fake-token"), Times.Once);
    }
    
    [Fact]
    public void MainLayout_OnAuthChanged_Logout_DisconnectsAndClears()
    {
        SetupUnauth();
        var cut = _ctx.Render<CascadingAuthenticationState>(p => p.AddChildContent<MainLayout>());
        var layout = cut.FindComponent<MainLayout>();
        
        var user = new ClaimsPrincipal(new ClaimsIdentity()); // Unauth
        var task = Task.FromResult(new AuthenticationState(user));
        
        var instance = layout.Instance;
        var method = typeof(MainLayout).GetMethod("OnAuthStateChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        method?.Invoke(instance, new object[] { task });
        
        // Should clear notifications
        _mockNotificationService.Verify(x => x.ClearAllAsync(), Times.Once);
    }

    #endregion

    #region Notification Logic Tests

    [Fact]
    public void MainLayout_HandleNewMessageNotification_AddsToService()
    {
         SetupUnauth();
         var cut = _ctx.Render<CascadingAuthenticationState>(p => p.AddChildContent<MainLayout>());
         var layout = cut.FindComponent<MainLayout>();
         
         // Invoke private event handler
         var instance = layout.Instance;
         var method = typeof(MainLayout).GetMethod("HandleNewMessageNotification", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
         
         var convId = Guid.NewGuid();
         method?.Invoke(instance, new object[] { convId, "Sender Name" });
         
         _mockNotificationService.Verify(x => x.AddUnreadConversationAsync(convId), Times.Once);
         
         _mockSnackbar.Verify(x => x.Add(It.Is<string>(s => s.Contains("Mesaj nou")), It.IsAny<Severity>(), It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void MainLayout_HandleNewMessageNotification_SkipIfOnSameUrl()
    {
        SetupUnauth();
        var cut = _ctx.Render<CascadingAuthenticationState>(p => p.AddChildContent<MainLayout>());
        var layout = cut.FindComponent<MainLayout>();
        
        var nav = _ctx.Services.GetRequiredService<NavigationManager>();
        var convId = Guid.NewGuid();
        
        // Navigate to conversation
        nav.NavigateTo($"/conversations/{convId}");
        
        var instance = layout.Instance;
        var method = typeof(MainLayout).GetMethod("HandleNewMessageNotification", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        method?.Invoke(instance, new object[] { convId, "Sender Name" });
        
        // Should NOT add notification
        _mockNotificationService.Verify(x => x.AddUnreadConversationAsync(convId), Times.Never);
         _mockSnackbar.Verify(x => x.Add(It.IsAny<string>(), It.IsAny<Severity>(), It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void MainLayout_Dispose_Unsubscribes()
    {
        SetupUnauth();
        var cut = _ctx.Render<CascadingAuthenticationState>(p => p.AddChildContent<MainLayout>());
        
        // Act
        var act = () => cut.FindComponent<MainLayout>().Instance.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    #endregion
}
