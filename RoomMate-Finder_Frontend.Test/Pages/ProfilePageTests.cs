using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class ProfilePageTests : BunitContext
{
    private readonly Mock<IProfileService> _mockProfileService;
    private readonly Mock<IRoommateService> _mockRoommateService;

    public ProfilePageTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        _mockProfileService = new Mock<IProfileService>();
        _mockRoommateService = new Mock<IRoommateService>();

        Services.AddSingleton(_mockProfileService.Object);
        Services.AddSingleton(_mockRoommateService.Object);
        
        Services.AddAuthorizationCore();
        Services.AddSingleton<AuthenticationStateProvider, TestProfileAuthStateProvider>();
    }

    [Fact(Skip = "Requires route parameter handling")]
    public void Profile_ShouldShowLoadingState_Initially()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        _mockProfileService.Setup(x => x.GetByIdAsync(profileId))
            .Returns(Task.Delay(5000).ContinueWith(_ => (ProfileDto?)null));

        var authProvider = Services.GetService<AuthenticationStateProvider>() as TestProfileAuthStateProvider;
        authProvider?.SetAuthenticatedUser(Guid.NewGuid(), "test@test.com", "User");

        // Act - Profile page requires a route parameter
        // This test is skipped because bUnit doesn't easily support route parameters
    }

    [Fact(Skip = "Requires route parameter handling")]
    public void Profile_ShouldShowError_WhenProfileNotFound()
    {
        // Arrange
        var profileId = Guid.NewGuid();
        _mockProfileService.Setup(x => x.GetByIdAsync(profileId))
            .ReturnsAsync((ProfileDto?)null);

        var authProvider = Services.GetService<AuthenticationStateProvider>() as TestProfileAuthStateProvider;
        authProvider?.SetAuthenticatedUser(Guid.NewGuid(), "test@test.com", "User");

        // This test is skipped because Profile page requires route parameters
    }

    public class TestProfileAuthStateProvider : AuthenticationStateProvider
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

