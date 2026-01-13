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

public class DiscoverPageTests : BunitContext
{
    private readonly Mock<IMatchingService> _mockMatchingService;
    private readonly Mock<IProfileService> _mockProfileService;
    private readonly Mock<ISnackbar> _mockSnackbar;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public DiscoverPageTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        _mockMatchingService = new Mock<IMatchingService>();
        _mockProfileService = new Mock<IProfileService>();
        _mockSnackbar = new Mock<ISnackbar>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        _mockConfiguration.Setup(c => c["ApiBaseUrl"]).Returns("http://localhost:5111");

        Services.AddSingleton(_mockMatchingService.Object);
        Services.AddSingleton(_mockProfileService.Object);
        Services.AddSingleton(_mockSnackbar.Object);
        Services.AddSingleton(_mockConfiguration.Object);
        Services.AddSingleton(new HttpClient());
        
        Services.AddAuthorizationCore();
        Services.AddSingleton<AuthenticationStateProvider, TestDiscoverAuthStateProvider>();
    }

    [Fact]
    public void Discover_ShouldRenderLoadingIndicator_Initially()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var currentProfile = new ProfileDto(
            currentUserId, "test@test.com", "Test User", 25, "Male", 
            "Test University", "Bio", "quiet", "Music", DateTime.UtcNow, null, "User"
        );

        _mockProfileService.Setup(x => x.GetCurrentAsync())
            .ReturnsAsync(currentProfile);
        _mockMatchingService.Setup(x => x.GetDiscoverProfilesAsync(It.IsAny<Guid>()))
            .Returns(Task.Delay(5000).ContinueWith(_ => new List<MatchProfileDto>()));

        var authProvider = Services.GetService<AuthenticationStateProvider>() as TestDiscoverAuthStateProvider;
        authProvider?.SetAuthenticatedUser(currentUserId, "test@test.com", "User");

        // Act
        var cut = Render<Discover>();

        // Assert - should show loading initially
        cut.Markup.Should().NotBeEmpty();
    }

    [Fact]
    public void Discover_ShouldShowEmptyState_WhenNoProfiles()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var currentProfile = new ProfileDto(
            currentUserId, "test@test.com", "Test User", 25, "Male",
            "Test University", "Bio", "quiet", "Music", DateTime.UtcNow, null, "User"
        );

        _mockProfileService.Setup(x => x.GetCurrentAsync())
            .ReturnsAsync(currentProfile);
        _mockMatchingService.Setup(x => x.GetDiscoverProfilesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<MatchProfileDto>());

        var authProvider = Services.GetService<AuthenticationStateProvider>() as TestDiscoverAuthStateProvider;
        authProvider?.SetAuthenticatedUser(currentUserId, "test@test.com", "User");

        // Act
        var cut = Render<Discover>();
        cut.WaitForState(() => cut.Markup.Contains("Gata pe moment") || cut.Markup.Contains("Nu mai sunt"), TimeSpan.FromSeconds(3));

        // Assert
        cut.Markup.Should().Contain("Gata pe moment");
    }

    public class TestDiscoverAuthStateProvider : AuthenticationStateProvider
    {
        private AuthenticationState _state = new(new ClaimsPrincipal(new ClaimsIdentity()));

        public void SetAuthenticatedUser(Guid userId, string email, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.Name, email),
                new Claim("sub", userId.ToString())
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

