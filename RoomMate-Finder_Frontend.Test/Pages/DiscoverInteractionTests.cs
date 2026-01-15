using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;
using RoomMate_Finder_Frontend.Models;
using Xunit;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class DiscoverInteractionTests : IAsyncLifetime
{
    private readonly TestContext _ctx = new();
    private readonly Mock<IMatchingService> _mockMatchingService;
    private readonly Mock<IProfileService> _mockProfileService;
    private readonly Mock<ISnackbar> _mockSnackbar;
    private readonly Mock<HttpClient> _mockHttp;
    
    // Test Data
    private readonly Guid _currentUserId = Guid.NewGuid();
    private readonly ProfileDto _currentUser;

    public DiscoverInteractionTests()
    {
        _ctx.Services.AddMudServices();
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        _mockMatchingService = new Mock<IMatchingService>();
        _mockProfileService = new Mock<IProfileService>();
        _mockSnackbar = new Mock<ISnackbar>();
        _mockHttp = new Mock<HttpClient>();
        
        _ctx.Services.AddSingleton(_mockMatchingService.Object);
        _ctx.Services.AddSingleton(_mockProfileService.Object);
        _ctx.Services.AddSingleton(_mockSnackbar.Object);
        _ctx.Services.AddSingleton(_mockHttp.Object); // Required by GetFullImageUrl

        // Setup Current User
        // (Guid Id, string Email, string FullName, int Age, string Gender, string University, string Bio, string Lifestyle, string Interests, DateTime CreatedAt, string? ProfilePicturePath, string Role = "User")
        _currentUser = new ProfileDto(
            _currentUserId, 
            "test@example.com", 
            "My Name", 
            20, 
            "Gender", 
            "University", 
            "Bio", 
            "Lifestyle", 
            "Interests", 
            DateTime.UtcNow, 
            null
        );

        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(_currentUser);
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _ctx.DisposeAsync();

    [Fact]
    public void Discover_LoadsProfiles_AndRendersCard()
    {
        // Arrange
        var profiles = new List<MatchProfileDto>
        {
            new MatchProfileDto(Guid.NewGuid(), "p1@test.com", "Profile One", 21, "Male", "Uni", "Bio", "Active", "Coding", 
                85.0, "High", DateTime.UtcNow, null)
        };
        _mockMatchingService.Setup(x => x.GetDiscoverProfilesAsync(_currentUserId)).ReturnsAsync(profiles);

        // Act
        var cut = _ctx.Render<Discover>();

        // Assert
        cut.WaitForState(() => cut.FindAll(".profile-card").Count > 0);
        cut.Find(".profile-card").TextContent.Should().Contain("Profile One");
        cut.Find(".profile-card").TextContent.Should().Contain("85%"); // Compatibility
    }

    [Fact]
    public void Discover_RefreshesWhenEmpty()
    {
         // Arrange
        _mockMatchingService.Setup(x => x.GetDiscoverProfilesAsync(_currentUserId)).ReturnsAsync(new List<MatchProfileDto>());

        // Act
        var cut = _ctx.Render<Discover>();

        // Assert
        cut.WaitForState(() => cut.FindAll(".mud-icon-size-large").Count > 0); // "Gata pe moment" icon
        cut.Markup.Should().Contain("Gata pe moment!");
        cut.Markup.Should().Contain("Nu mai sunt profiluri disponibile.");
    }

    [Fact]
    public async Task Discover_LikeProfile_CallsService_AndRemovesCard()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var profiles = new List<MatchProfileDto>
        {
            new MatchProfileDto(targetId, "p1@test.com", "Profile One", 21, "Male", "Uni", "Bio", "Active", "Coding", 
                85.0, "High", DateTime.UtcNow, null)
        };
        _mockMatchingService.Setup(x => x.GetDiscoverProfilesAsync(_currentUserId)).ReturnsAsync(profiles);
        _mockMatchingService.Setup(x => x.LikeProfileAsync(_currentUserId, targetId))
            .ReturnsAsync(new LikeResponseDto(true, "Liked", false, null));

        var cut = _ctx.Render<Discover>();
        cut.WaitForState(() => cut.FindAll(".btn-like").Count > 0);

        // Act
        var likeBtn = cut.Find("button.btn-like");
        await cut.InvokeAsync(() => likeBtn.Click());

        // Assert
        _mockMatchingService.Verify(x => x.LikeProfileAsync(_currentUserId, targetId), Times.Once);
        
        // Should show empty state now (since we removed the only profile)
        cut.WaitForState(() => cut.Markup.Contains("Gata pe moment!"));
    }

    [Fact]
    public async Task Discover_PassProfile_CallsService_AndRemovesCard()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var profiles = new List<MatchProfileDto>
        {
            new MatchProfileDto(targetId, "p1@test.com", "Profile One", 21, "Male", "Uni", "Bio", "Active", "Coding", 
                85.0, "High", DateTime.UtcNow, null)
        };
        _mockMatchingService.Setup(x => x.GetDiscoverProfilesAsync(_currentUserId)).ReturnsAsync(profiles);
        _mockMatchingService.Setup(x => x.PassProfileAsync(_currentUserId, targetId))
            .ReturnsAsync(new PassResponseDto(true, "Passed"));

        var cut = _ctx.Render<Discover>();
        cut.WaitForState(() => cut.FindAll(".btn-pass").Count > 0);

        // Act
        var passBtn = cut.Find("button.btn-pass");
        await cut.InvokeAsync(() => passBtn.Click());

        // Assert
        _mockMatchingService.Verify(x => x.PassProfileAsync(_currentUserId, targetId), Times.Once);
        
        // Should show empty state
        cut.WaitForState(() => cut.Markup.Contains("Gata pe moment!"));
    }

    [Fact]
    public async Task Discover_Match_ShowsSnackbar_And_Navigates()
    {
        // Arrange
        var targetId = Guid.NewGuid();
        var profiles = new List<MatchProfileDto>
        {
            new MatchProfileDto(targetId, "p1@test.com", "Profile One", 21, "Male", "Uni", "Bio", "Active", "Coding", 
                85.0, "High", DateTime.UtcNow, null)
        };
        _mockMatchingService.Setup(x => x.GetDiscoverProfilesAsync(_currentUserId)).ReturnsAsync(profiles);
        _mockMatchingService.Setup(x => x.LikeProfileAsync(_currentUserId, targetId))
            .ReturnsAsync(new LikeResponseDto(true, "Match!", true, Guid.NewGuid())); // IsMatch = true

        var navigation = _ctx.Services.GetRequiredService<NavigationManager>();
        
        var cut = _ctx.Render<Discover>();
        cut.WaitForState(() => cut.FindAll(".btn-like").Count > 0);

        // Act
        var likeBtn = cut.Find("button.btn-like");
        await cut.InvokeAsync(() => likeBtn.Click());

        // Assert
        _mockSnackbar.Verify(x => x.Add("Match nou!", Severity.Success, null, null), Times.Once);
        navigation.Uri.Should().EndWith("/my-matches");
    }
}
