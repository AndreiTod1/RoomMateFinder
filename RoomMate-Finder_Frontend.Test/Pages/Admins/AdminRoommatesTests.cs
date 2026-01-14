using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages.Admins;
using RoomMate_Finder_Frontend.Services;
using System.Security.Claims;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Pages.Admins;

public class AdminRoommatesTests : BunitContext, IAsyncLifetime
{
    private readonly Mock<IRoommateService> _mockRoommateService;
    private readonly Mock<ISnackbar> _mockSnackbar;
    private readonly AuthenticationState _authState;

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    public AdminRoommatesTests()
    {
        _mockRoommateService = new Mock<IRoommateService>();
        _mockSnackbar = new Mock<ISnackbar>();

        Services.AddMudServices();
        Services.AddSingleton(_mockRoommateService.Object);
        Services.AddSingleton(_mockSnackbar.Object);
        Services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        // Setup manual auth
        Services.AddAuthorizationCore();
        var claims = new[] 
        { 
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Admin"), 
            new Claim(ClaimTypes.Name, "admin") 
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        _authState = new AuthenticationState(user);

        var mockAuthProvider = new Mock<AuthenticationStateProvider>();
        mockAuthProvider.Setup(x => x.GetAuthenticationStateAsync()).ReturnsAsync(_authState);
        Services.AddSingleton(mockAuthProvider.Object);

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private void RenderProviders()
    {
        Render<MudPopoverProvider>();
        Render<MudDialogProvider>();
    }

    [Fact]
    public void AdminRoommates_ComponentExists()
    {
        var componentType = typeof(AdminRoommates);
        componentType.Should().NotBeNull();
    }

    [Fact]
    public void AdminRoommates_HasAuthorizeAttribute()
    {
        var authorizeAttribute = typeof(AdminRoommates)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false)
            .FirstOrDefault() as Microsoft.AspNetCore.Authorization.AuthorizeAttribute;
        
        authorizeAttribute.Should().NotBeNull();
    }

    [Fact]
    public void AdminRoommates_HasCorrectPageRoute()
    {
        var routeAttribute = typeof(AdminRoommates)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Components.RouteAttribute), false)
            .FirstOrDefault() as Microsoft.AspNetCore.Components.RouteAttribute;
        
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Contain("admin");
    }

    [Fact]
    public void AdminRoommates_RoommateServiceRegistered()
    {
        Services.GetService<IRoommateService>().Should().NotBeNull();
    }

    [Fact]
    public void AdminRoommates_SnackbarRegistered()
    {
        Services.GetService<ISnackbar>().Should().NotBeNull();
    }

    [Fact]
    public void AdminRoommates_ImplementsComponentBase()
    {
        typeof(AdminRoommates)
            .IsSubclassOf(typeof(Microsoft.AspNetCore.Components.ComponentBase))
            .Should().BeTrue();
    }
}
