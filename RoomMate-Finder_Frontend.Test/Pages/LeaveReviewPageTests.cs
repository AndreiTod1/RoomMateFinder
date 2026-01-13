using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;
using RoomMate_Finder_Frontend.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class LeaveReviewPageTests : BunitContext
{
    private readonly Mock<IReviewService> _mockReviewService;
    private readonly Mock<IProfileService> _mockProfileService;
    private readonly Mock<ISnackbar> _mockSnackbar;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public LeaveReviewPageTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        _mockReviewService = new Mock<IReviewService>();
        _mockProfileService = new Mock<IProfileService>();
        _mockSnackbar = new Mock<ISnackbar>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        _mockConfiguration.Setup(c => c["ApiBaseUrl"]).Returns("http://localhost:5111");

        Services.AddSingleton(_mockReviewService.Object);
        Services.AddSingleton(_mockProfileService.Object);
        Services.AddSingleton(_mockSnackbar.Object);
        Services.AddSingleton(_mockConfiguration.Object);
        
        Services.AddAuthorizationCore();
        Services.AddSingleton<AuthenticationStateProvider, TestLeaveReviewAuthStateProvider>();
    }

    [Fact(Skip = "Requires CascadingAuthenticationState setup")]
    public void LeaveReview_ShouldRenderTitle()
    {
        // Arrange
        var currentUserId = Guid.NewGuid();
        var currentProfile = new ProfileDto(
            currentUserId, "test@test.com", "Test User", 25, "Male",
            "Test University", "Bio", "quiet", "Music", DateTime.UtcNow, null, "User"
        );

        _mockProfileService.Setup(x => x.GetCurrentAsync())
            .ReturnsAsync(currentProfile);
        _mockReviewService.Setup(x => x.GetMatchesForReview(It.IsAny<Guid>()))
            .ReturnsAsync(new List<UserMatchDto>());

        var authProvider = Services.GetService<AuthenticationStateProvider>() as TestLeaveReviewAuthStateProvider;
        authProvider?.SetAuthenticatedUser(currentUserId, "test@test.com", "User");

        // Act
        var cut = Render<LeaveReview>();

        // Assert - page should render without errors
        cut.Markup.Should().NotBeEmpty();
    }

    [Fact(Skip = "Requires CascadingAuthenticationState setup")]
    public void LeaveReview_ShouldLoadMatchesForReview()
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
                Guid.NewGuid(), Guid.NewGuid(), "match@test.com", "Match User", 24, "Female",
                "University", "Bio", "social", "Sports", DateTime.UtcNow, true
            )
        };

        _mockProfileService.Setup(x => x.GetCurrentAsync())
            .ReturnsAsync(currentProfile);
        _mockReviewService.Setup(x => x.GetMatchesForReview(It.IsAny<Guid>()))
            .ReturnsAsync(matches);

        var authProvider = Services.GetService<AuthenticationStateProvider>() as TestLeaveReviewAuthStateProvider;
        authProvider?.SetAuthenticatedUser(currentUserId, "test@test.com", "User");

        // Act
        var cut = Render<LeaveReview>();
        
        // Assert - Verify that GetMatchesForReview was called
        _mockReviewService.Verify(x => x.GetMatchesForReview(It.IsAny<Guid>()), Times.AtLeastOnce);
    }

    public class TestLeaveReviewAuthStateProvider : AuthenticationStateProvider
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

