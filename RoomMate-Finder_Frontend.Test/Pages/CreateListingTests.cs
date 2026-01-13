using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;
using System.Security.Claims;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class CreateListingTests : BunitContext
{
    private readonly Mock<IListingService> _mockListingService;
    private readonly Mock<ISnackbar> _mockSnackbar;

    public CreateListingTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        
        _mockListingService = new Mock<IListingService>();
        _mockSnackbar = new Mock<ISnackbar>();
        
        Services.AddSingleton(_mockListingService.Object);
        Services.AddSingleton(_mockSnackbar.Object);
        
        Services.AddAuthorizationCore();
        // Register our custom mock provider
        Services.AddSingleton<AuthenticationStateProvider, CustomMockAuthStateProvider>();
    }

    [Fact(Skip = "Requires advanced MudBlazor JS mocking (Popover/Select/FileUpload)")]
    public void CreateListing_RendersCorrectly_ForAdmin()
    {
        // Arrange
        var authProvider = Services.GetService<AuthenticationStateProvider>() as CustomMockAuthStateProvider;
        authProvider?.Authenticate("admin", new[] { "Admin" });

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<MudPopoverProvider>(0);
            builder.CloseComponent();
            builder.OpenComponent<CreateListing>(1);
            builder.CloseComponent();
        });

        // Assert
        cut.Markup.Should().Contain("Post a New Room", "Page title should be visible for Admin");
        // Check for presence of key form fields
        cut.FindComponents<MudTextField<string>>().Should().NotBeEmpty();
    }

    [Fact(Skip = "Requires advanced MudBlazor JS mocking")]
    public void CreateListing_ShowsNothing_IfNotAdmin()
    {
        // Arrange
        var authProvider = Services.GetService<AuthenticationStateProvider>() as CustomMockAuthStateProvider;
        authProvider?.Authenticate("user", new[] { "User" });

        // Act
        var cut = Render(builder =>
        {
            builder.OpenComponent<CreateListing>(0);
            builder.CloseComponent();
        });

        // Assert
        cut.Markup.Should().NotContain("Post a New Room");
    }

    // Explicit Clean Mock Class
    public class CustomMockAuthStateProvider : AuthenticationStateProvider
    {
        private AuthenticationState _authState;

        public CustomMockAuthStateProvider()
        {
            // Default to anonymous
            _authState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public void Authenticate(string username, string[] roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username)
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            _authState = new AuthenticationState(new ClaimsPrincipal(identity));
            
            NotifyAuthenticationStateChanged(Task.FromResult(_authState));
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(_authState);
        }
    }
}
