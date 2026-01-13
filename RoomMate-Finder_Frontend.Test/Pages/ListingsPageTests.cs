using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;
using RoomMate_Finder_Frontend.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class ListingsPageTests : BunitContext
{
    private readonly Mock<IListingService> _mockListingService;
    private readonly Mock<ISnackbar> _mockSnackbar;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public ListingsPageTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        _mockListingService = new Mock<IListingService>();
        _mockSnackbar = new Mock<ISnackbar>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup configuration to return a default API base URL
        _mockConfiguration.Setup(c => c["ApiBaseUrl"]).Returns("http://localhost:5111");

        Services.AddSingleton(_mockListingService.Object);
        Services.AddSingleton(_mockSnackbar.Object);
        Services.AddSingleton(_mockConfiguration.Object);
        
        Services.AddAuthorizationCore();
        Services.AddSingleton<AuthenticationStateProvider, TestAuthStateProvider>();
    }

    [Fact]
    public void Listings_ShouldRenderTitle()
    {
        // Arrange
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(new List<ListingSummaryDto>(), 0, 1, 12));
        
        var authProvider = Services.GetService<AuthenticationStateProvider>() as TestAuthStateProvider;
        authProvider?.SetAuthenticatedUser("test@test.com", "User");

        // Act
        var cut = Render<Listings>();

        // Assert
        cut.Markup.Should().Contain("Available Rooms");
    }

    [Fact]
    public void Listings_ShouldShowNoRoomsMessage_WhenEmpty()
    {
        // Arrange
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(new List<ListingSummaryDto>(), 0, 1, 12));
        
        var authProvider = Services.GetService<AuthenticationStateProvider>() as TestAuthStateProvider;
        authProvider?.SetAuthenticatedUser("test@test.com", "User");

        // Act
        var cut = Render<Listings>();
        cut.WaitForState(() => !cut.Markup.Contains("Loading"));

        // Assert
        cut.Markup.Should().Contain("No rooms found");
    }

    // Helper class for authentication
    public class TestAuthStateProvider : AuthenticationStateProvider
    {
        private AuthenticationState _state = new(new ClaimsPrincipal(new ClaimsIdentity()));

        public void SetAuthenticatedUser(string email, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.Name, email)
            };
            var identity = new ClaimsIdentity(claims, "Test");
            _state = new AuthenticationState(new ClaimsPrincipal(identity));
            NotifyAuthenticationStateChanged(Task.FromResult(_state));
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(_state);
        }
    }
}

