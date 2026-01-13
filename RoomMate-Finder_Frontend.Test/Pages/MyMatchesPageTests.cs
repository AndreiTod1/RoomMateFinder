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

public class MyMatchesPageTests : BunitContext
{
    private readonly Mock<IMatchingService> _mockMatchingService;
    private readonly Mock<IProfileService> _mockProfileService;
    private readonly Mock<IConversationService> _mockConversationService;
    private readonly Mock<ISnackbar> _mockSnackbar;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public MyMatchesPageTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        _mockMatchingService = new Mock<IMatchingService>();
        _mockProfileService = new Mock<IProfileService>();
        _mockConversationService = new Mock<IConversationService>();
        _mockSnackbar = new Mock<ISnackbar>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        _mockConfiguration.Setup(c => c["ApiBaseUrl"]).Returns("http://localhost:5111");

        Services.AddSingleton(_mockMatchingService.Object);
        Services.AddSingleton(_mockProfileService.Object);
        Services.AddSingleton(_mockConversationService.Object);
        Services.AddSingleton(_mockSnackbar.Object);
        Services.AddSingleton(_mockConfiguration.Object);
        
        Services.AddAuthorizationCore();
        Services.AddSingleton<AuthenticationStateProvider, TestMatchesAuthStateProvider>();
    }

    [Fact]
    public void MyMatches_ShouldRenderTitle()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var currentProfile = new ProfileDto(
            currentUserId, "test@test.com", "Test User", 25, "Male",
            "Test University", "Bio", "quiet", "Music", DateTime.UtcNow, null, "User"
        );

        _mockProfileService.Setup(x => x.GetCurrentAsync())
            .ReturnsAsync(currentProfile);
        _mockMatchingService.Setup(x => x.GetMyMatchesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<UserMatchDto>());

        var authProvider = Services.GetService<AuthenticationStateProvider>() as TestMatchesAuthStateProvider;
        authProvider?.SetAuthenticatedUser(currentUserId, "test@test.com", "User");

        // Act
        var cut = Render<MyMatches>();

        // Assert
        cut.Markup.Should().Contain("Match-urile Mele");
    }

    [Fact]
    public void MyMatches_ShouldShowEmptyState_WhenNoMatches()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var currentProfile = new ProfileDto(
            currentUserId, "test@test.com", "Test User", 25, "Male",
            "Test University", "Bio", "quiet", "Music", DateTime.UtcNow, null, "User"
        );

        _mockProfileService.Setup(x => x.GetCurrentAsync())
            .ReturnsAsync(currentProfile);
        _mockMatchingService.Setup(x => x.GetMyMatchesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<UserMatchDto>());

        var authProvider = Services.GetService<AuthenticationStateProvider>() as TestMatchesAuthStateProvider;
        authProvider?.SetAuthenticatedUser(currentUserId, "test@test.com", "User");

        // Act
        var cut = Render<MyMatches>();
        cut.WaitForState(() => cut.Markup.Contains("niciun match") || cut.Markup.Contains("Încă nu ai"), TimeSpan.FromSeconds(3));

        // Assert
        cut.Markup.Should().Contain("Încă nu ai niciun match");
    }

    [Fact(Skip = "MudBlazor KeyInterceptorService async disposal issue")]
    public void MyMatches_ShouldShowMatches_WhenAvailable()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var currentProfile = new ProfileDto(
            currentUserId, "test@test.com", "Test User", 25, "Male",
            "Test University", "Bio", "quiet", "Music", DateTime.UtcNow, null, "User"
        );

        var matches = new List<UserMatchDto>
        {
            new UserMatchDto(
                Guid.NewGuid(), Guid.NewGuid(), "match1@test.com", "Match One", 24, "Female",
                "University A", "Bio here", "social", "Sports", DateTime.UtcNow, true
            ),
            new UserMatchDto(
                Guid.NewGuid(), Guid.NewGuid(), "match2@test.com", "Match Two", 26, "Male",
                "University B", "Another bio", "quiet", "Music", DateTime.UtcNow, true
            )
        };

        _mockProfileService.Setup(x => x.GetCurrentAsync())
            .ReturnsAsync(currentProfile);
        _mockMatchingService.Setup(x => x.GetMyMatchesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(matches);

        var authProvider = Services.GetService<AuthenticationStateProvider>() as TestMatchesAuthStateProvider;
        authProvider?.SetAuthenticatedUser(currentUserId, "test@test.com", "User");

        // Act
        var cut = Render<MyMatches>();
        cut.WaitForState(() => cut.Markup.Contains("Match One") || cut.Markup.Contains("Match Two"), TimeSpan.FromSeconds(3));

        // Assert
        cut.Markup.Should().Contain("Match One");
        cut.Markup.Should().Contain("Match Two");
    }

    public class TestMatchesAuthStateProvider : AuthenticationStateProvider
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

