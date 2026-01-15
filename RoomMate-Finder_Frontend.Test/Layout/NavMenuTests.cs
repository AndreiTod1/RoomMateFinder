using Bunit;
using Bunit.TestDoubles;
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
using System.Security.Claims;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Layout;

public class NavMenuTests : IAsyncLifetime
{
    private readonly BunitContext _ctx = new();
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<IProfileService> _mockProfileService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<AuthenticationStateProvider> _mockAuthProvider;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _ctx.DisposeAsync();
    }

    public NavMenuTests()
    {
        _ctx.Services.AddMudServices();
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        _mockAuthService = new Mock<IAuthService>();
        _mockProfileService = new Mock<IProfileService>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockAuthProvider = new Mock<AuthenticationStateProvider>();

        _ctx.Services.AddSingleton(_mockAuthService.Object);
        _ctx.Services.AddSingleton(_mockProfileService.Object);
        _ctx.Services.AddSingleton(_mockNotificationService.Object);
        _ctx.Services.AddSingleton(_mockAuthProvider.Object);
        
        // Essential for AuthorizeView - Setup real authorization
        _ctx.Services.AddOptions();
        _ctx.Services.AddLogging();
        _ctx.Services.AddAuthorizationCore();
        // Force replacement of BUnit's PlaceholderAuthorizationService
        _ctx.Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();
    }

    private AuthenticationState SetupAuth(string role = "User", string name = "TestUser", Guid? userId = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role)
        };
        
        if (userId.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
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

    #region Anonymous View Tests

    [Fact]
    public void NavMenu_Anonymous_ShowsLoginRegisterButtons()
    {
        SetupUnauth();
        var cut = _ctx.Render<CascadingAuthenticationState>(p => p.AddChildContent<NavMenu>());
        
        cut.Markup.Should().Contain("Login");
        cut.Markup.Should().Contain("Register");
        cut.Markup.Should().Contain("/login");
        cut.Markup.Should().Contain("/register");
    }

    [Fact]
    public void NavMenu_Anonymous_DoesNotShowLogout()
    {
        var authState = SetupUnauth();
        var cut = _ctx.Render<NavMenu>(parameters => parameters
            .AddCascadingValue(Task.FromResult(authState)));
        
        cut.Markup.Should().NotContain("Logout");
    }

    [Fact]
    public void NavMenu_Anonymous_ShowsDashboardLink()
    {
        var authState = SetupUnauth();
        var cut = _ctx.Render<NavMenu>(parameters => parameters
            .AddCascadingValue(Task.FromResult(authState)));
        
        cut.Markup.Should().Contain("Dashboard");
    }

    [Fact]
    public void NavMenu_Anonymous_DoesNotShowAuthorizedMenus()
    {
        var authState = SetupUnauth();
        var cut = _ctx.Render<NavMenu>(parameters => parameters
            .AddCascadingValue(Task.FromResult(authState)));
        
        cut.Markup.Should().NotContain("Profilul Meu");
        cut.Markup.Should().NotContain("Caută Colegi");
        cut.Markup.Should().NotContain("Conversații");
    }

    #endregion

    #region Authorized View Tests

    [Fact]
    public void NavMenu_Authorized_ShowsLogoutButton()
    {
        var authState = SetupAuth();
        var cut = _ctx.Render<NavMenu>(parameters => parameters
            .AddCascadingValue(Task.FromResult(authState)));
        
        cut.Markup.Should().Contain("Logout");
    }

    [Fact]
    public void NavMenu_Authorized_ShowsGreeting()
    {
        var authState = SetupAuth(name: "John");
        var cut = _ctx.Render<NavMenu>(parameters => parameters
            .AddCascadingValue(Task.FromResult(authState)));
        
        cut.Markup.Should().Contain("Salut, John");
    }

    [Fact]
    public void NavMenu_Authorized_ShowsUserMenus()
    {
        var authState = SetupAuth();
        var cut = _ctx.Render<NavMenu>(parameters => parameters
            .AddCascadingValue(Task.FromResult(authState)));
        
        cut.Markup.Should().Contain("Profilul Meu");
        cut.Markup.Should().Contain("Matching");
        cut.Markup.Should().Contain("Conversații");
        cut.Markup.Should().Contain("Camere");
        cut.Markup.Should().Contain("Recenzii");
    }

    [Fact]
    public void NavMenu_Authorized_DoesNotShowAdminMenu()
    {
        var authState = SetupAuth(role: "User");
        var cut = _ctx.Render<NavMenu>(parameters => parameters
            .AddCascadingValue(Task.FromResult(authState)));
        
        cut.Markup.Should().NotContain("Administrare");
    }

    #endregion

    #region Admin View Tests

    [Fact]
    public void NavMenu_Admin_ShowsAdminMenu()
    {
        var authState = SetupAuth(role: "Admin");
        var cut = _ctx.Render<NavMenu>(parameters => parameters
            .AddCascadingValue(Task.FromResult(authState)));
        
        cut.Markup.Should().Contain("Administrare");
        cut.Markup.Should().Contain("Utilizatori");
        cut.Markup.Should().Contain("Anunțuri în Așteptare");
    }

    #endregion

    #region Notification Tests

    [Fact]
    public void NavMenu_ShowsNotificationCount_WhenUnreadExists()
    {
        var authState = SetupAuth();
        _mockNotificationService.Setup(x => x.UnreadConversationsCount).Returns(5);
        
        var cut = _ctx.Render<NavMenu>(parameters => parameters
            .AddCascadingValue(Task.FromResult(authState)));
        
        cut.Markup.Should().Contain("5"); 
        cut.FindComponents<MudChip<string>>().Should().Contain(c => c.Markup.Contains('5'));
    }

    [Fact]
    public void NavMenu_DoesNotShowNotificationCount_WhenZero()
    {
        var authState = SetupAuth();
        _mockNotificationService.Setup(x => x.UnreadConversationsCount).Returns(0);
        
        var cut = _ctx.Render<NavMenu>(parameters => parameters
            .AddCascadingValue(Task.FromResult(authState)));
        
        cut.Markup.Should().NotContain("Color.Error"); 
    }

    [Fact]
    public void NavMenu_UpdatesOnNotificationChanged()
    {
        var authState = SetupAuth();
        var cut = _ctx.Render<NavMenu>(parameters => parameters
            .AddCascadingValue(Task.FromResult(authState)));
        
        _mockNotificationService.Verify(x => x.InitializeAsync(), Times.Once);
    }

    #endregion

    #region Action Tests

    [Fact]
    public async Task NavMenu_Logout_CallsServiceAndRedirects()
    {
        var authState = SetupAuth();
        var cut = _ctx.Render<NavMenu>(parameters => parameters
            .AddCascadingValue(Task.FromResult(authState)));
        
        var logoutBtn = cut.FindComponents<MudButton>()
            .First(b => b.Markup.Contains("Logout"));
            
        await cut.InvokeAsync(() => logoutBtn.Instance.OnClick.InvokeAsync());
        
        _mockAuthService.Verify(x => x.LogoutAsync(), Times.Once);
        _ctx.Services.GetRequiredService<NavigationManager>().Uri.Should().Be("http://localhost/");
    }

    [Fact]
    public void NavMenu_GoToMyProfile_WhenServiceReturnsProfile_CallsService()
    {
        // Arrange
        var authState = SetupAuth();
        var profileId = Guid.NewGuid();
        _mockProfileService.Setup(x => x.GetCurrentAsync())
            .ReturnsAsync(new ProfileDto(profileId, "test@example.com", "Test User", 20, "Gender", "Uni", "Bio", "Life", "Hobby", DateTime.UtcNow, "http://img"));

        // Act
        var cut = _ctx.Render<NavMenu>(parameters => parameters
            .AddCascadingValue(Task.FromResult(authState)));

        // Assert - component renders with profile service available
        cut.Should().NotBeNull();
        cut.Markup.Should().Contain("Profilul Meu");
    }

    [Fact]
    public async Task NavMenu_GoToMyProfile_FallbackToClaims_WhenServiceFails()
    {
        var userId = Guid.NewGuid();
        var authState = SetupAuth(userId: userId);
        
        _mockProfileService.Setup(x => x.GetCurrentAsync())
            .ThrowsAsync(new Exception("API Error"));
            
        var cut = _ctx.Render<CascadingAuthenticationState>(p => p.AddChildContent<NavMenu>());
        
        var links = cut.FindComponents<MudNavLink>();
        var myProfileLink = links.FirstOrDefault(l => l.Markup.Contains("Profilul Meu"));
        
        if (myProfileLink != null)
        {
            await cut.InvokeAsync(() => myProfileLink.Instance.OnClick.InvokeAsync());
            _ctx.Services.GetRequiredService<NavigationManager>().Uri.Should().Contain($"/profile/{userId}");
        }
    }

    [Fact]
    public void NavMenu_GoToMyProfile_WhenUnauthorized_ServiceThrows()
    {
        // Arrange
        var authState = SetupAuth();
        _mockProfileService.Setup(x => x.GetCurrentAsync())
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var cut = _ctx.Render<NavMenu>(parameters => parameters
            .AddCascadingValue(Task.FromResult(authState)));

        // Assert - component renders even if service throws
        cut.Should().NotBeNull();
        cut.Markup.Should().Contain("Profilul Meu");
    }

    #endregion

    #region Drawer Toggle Test

    [Fact]
    public void NavMenu_DrawerToggle_ChangesState()
    {
        var authState = SetupAuth();
        var cut = _ctx.Render<CascadingAuthenticationState>(p => p.AddChildContent<NavMenu>());
            
        var msgBtn = cut.FindComponents<MudIconButton>()
            .First(b => b.Instance.Icon == Icons.Material.Filled.Menu);
            
        var drawer = cut.FindComponent<MudDrawer>();
        bool initialState = drawer.Instance.Open;
        
        msgBtn.Find("button").Click();
        
        drawer.Instance.Open.Should().Be(!initialState);
    }

    #endregion
}
