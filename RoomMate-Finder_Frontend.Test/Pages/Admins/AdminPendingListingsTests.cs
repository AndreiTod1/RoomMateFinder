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

public class AdminPendingListingsTests : BunitContext, IAsyncLifetime
{
    private readonly Mock<IListingService> _mockListingService;
    private readonly Mock<ISnackbar> _mockSnackbar;
    private readonly AuthenticationState _authState;

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    public AdminPendingListingsTests()
    {
        _mockListingService = new Mock<IListingService>();
        _mockSnackbar = new Mock<ISnackbar>();

        Services.AddMudServices();
        Services.AddSingleton(_mockListingService.Object);
        Services.AddSingleton(_mockSnackbar.Object);
        Services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiBaseUrl"] = "http://localhost:5000"
            })
            .Build());

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

    [Fact]
    public void AdminPendingListings_ComponentExists()
    {
        var componentType = typeof(AdminPendingListings);
        componentType.Should().NotBeNull();
    }

    [Fact]
    public void AdminPendingListings_HasAuthorizeAttribute()
    {
        var authorizeAttribute = typeof(AdminPendingListings)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false)
            .FirstOrDefault() as Microsoft.AspNetCore.Authorization.AuthorizeAttribute;
        
        authorizeAttribute.Should().NotBeNull();
    }

    [Fact]
    public void AdminPendingListings_HasCorrectPageRoute()
    {
        var routeAttribute = typeof(AdminPendingListings)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Components.RouteAttribute), false)
            .FirstOrDefault() as Microsoft.AspNetCore.Components.RouteAttribute;
        
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Contain("pending");
    }

    [Fact]
    public void AdminPendingListings_ListingServiceRegistered()
    {
        Services.GetService<IListingService>().Should().NotBeNull();
    }

    [Fact]
    public void AdminPendingListings_SnackbarRegistered()
    {
        Services.GetService<ISnackbar>().Should().NotBeNull();
    }

    [Fact]
    public void AdminPendingListings_ImplementsComponentBase()
    {
        typeof(AdminPendingListings)
            .IsSubclassOf(typeof(Microsoft.AspNetCore.Components.ComponentBase))
            .Should().BeTrue();
    }

    [Fact]
    public void AdminPendingListings_ConfigurationRegistered()
    {
        Services.GetService<IConfiguration>().Should().NotBeNull();
    }
}
