using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;
using RoomMate_Finder_Frontend.Models;
using System.Security.Claims;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class LeaveReviewTests : IAsyncLifetime
{
    private readonly BunitContext _ctx = new();
    private readonly Mock<IReviewService> _mockReviewService;
    private readonly Mock<IProfileService> _mockProfileService;
    private readonly Mock<ISnackbar> _mockSnackbar;
    private readonly Mock<AuthenticationStateProvider> _mockAuthProvider;
    private AuthenticationState _authState;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _ctx.DisposeAsync();
    }

    public LeaveReviewTests()
    {
        _mockReviewService = new Mock<IReviewService>();
        _mockProfileService = new Mock<IProfileService>();
        _mockSnackbar = new Mock<ISnackbar>();
        _mockAuthProvider = new Mock<AuthenticationStateProvider>();

        _ctx.Services.AddMudServices();
        _ctx.Services.AddSingleton(_mockReviewService.Object);
        _ctx.Services.AddSingleton(_mockProfileService.Object);
        _ctx.Services.AddSingleton(_mockSnackbar.Object);
        _ctx.Services.AddSingleton(_mockAuthProvider.Object);
        
        // Fix MudSnackbarProvider NPE
        _mockSnackbar.Setup(x => x.Configuration).Returns(new SnackbarConfiguration());
        
        _ctx.Services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiBaseUrl"] = "http://localhost:5000"
            })
            .Build());

        // Authorization Fix
        _ctx.Services.AddOptions();
        _ctx.Services.AddLogging();
        _ctx.Services.AddAuthorizationCore();
        _ctx.Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();

        // Default: Authorized
        var claims = new[] 
        { 
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "User"), 
            new Claim(ClaimTypes.Name, "testuser") 
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        _authState = new AuthenticationState(user);
        
        _mockAuthProvider.Setup(x => x.GetAuthenticationStateAsync()).ReturnsAsync(_authState);
        
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<CascadingAuthenticationState> RenderComponent()
    {
        return _ctx.Render<CascadingAuthenticationState>(p => p.AddChildContent<LeaveReview>());
    }
    
    private void RenderProviders()
    {
        _ctx.Render<MudPopoverProvider>();
        _ctx.Render<MudDialogProvider>();
        _ctx.Render<MudSnackbarProvider>();
    }

    [Fact]
    public void LeaveReview_NotAuthorized_ShowsMessage()
    {
        // Arrange
        var unauthState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        _mockAuthProvider.Setup(x => x.GetAuthenticationStateAsync()).ReturnsAsync(unauthState);
        
        // Act
        // Use wrapper. It pulls from Provider.
        var cut = _ctx.Render<CascadingAuthenticationState>(p => p.AddChildContent<LeaveReview>());

        // Assert
        cut.Markup.Should().Contain("Trebuie să fii autentificat");
    }

    [Fact]
    public void LeaveReview_Loading_ComponentRenders()
    {
        // Arrange
        var tcs = new TaskCompletionSource<ProfileDto?>();
        _mockProfileService.Setup(x => x.GetCurrentAsync()).Returns(tcs.Task);
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert - component renders during loading state
        cut.Should().NotBeNull();
        
        // Cleanup
        tcs.SetResult(null);
    }
    
    [Fact]
    public void LeaveReview_NoMatches_ShowsMessage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = new ProfileDto(userId, "me@test.com", "Me", 25, "M", "Uni", "Bio", "Life", "Int", DateTime.UtcNow, null, "User");
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(profile);
        _mockReviewService.Setup(x => x.GetMatchesForReview(userId)).ReturnsAsync(new List<UserMatchDto>());
        
        RenderProviders();

        // Act
        var cut = RenderComponent();
        cut.WaitForState(() => cut.Markup.Contains("Nu ai niciun match"));

        // Assert
        cut.Markup.Should().Contain("Nu ai niciun match pentru a lăsa o recenzie");
    }

    [Fact]
    public void LeaveReview_WithMatches_RendersCards()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = new ProfileDto(userId, "me@test.com", "Me", 25, "M", "Uni", "Bio", "Life", "Int", DateTime.UtcNow, null, "User");
        var matches = new List<UserMatchDto>
        {
            new UserMatchDto(
                Guid.NewGuid(), 
                Guid.NewGuid(), 
                "match@test.com", 
                "Match Person", 
                25, "M", "Uni", "Bio", "Style", "Interests", DateTime.UtcNow, true, "/img.jpg"
            )
        };

        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(profile);
        _mockReviewService.Setup(x => x.GetMatchesForReview(userId)).ReturnsAsync(matches);
        
        RenderProviders();

        // Act
        var cut = RenderComponent();
        cut.WaitForAssertion(() => cut.Markup.Contains("Match Person"));

        // Assert
        cut.Markup.Should().Contain("Match Person");
        cut.FindComponents<MudRating>().Should().NotBeEmpty();
    }

    [Fact]
    public async Task LeaveReview_Submit_CallsServiceAndRemovesCard()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var matchUserId = Guid.NewGuid();
        var profile = new ProfileDto(userId, "me@test.com", "Me", 25, "M", "Uni", "Bio", "Life", "Int", DateTime.UtcNow, null, "User");
        var matches = new List<UserMatchDto>
        {
            new UserMatchDto(
                Guid.NewGuid(), // MatchId
                matchUserId,
                "match@test.com",
                "Match Person",
                25, "M", "Uni", "Bio", "Style", "Interests", DateTime.UtcNow, true, "/img.jpg"
            )
        };

        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(profile);
        _mockReviewService.Setup(x => x.GetMatchesForReview(userId)).ReturnsAsync(matches);
        
        RenderProviders();

        var cut = RenderComponent();
        cut.WaitForAssertion(() => cut.Markup.Contains("Match Person"));

        // Act
        // Set Rating
        var rating = cut.FindComponent<MudRating>();
        // Use DOM click on the 5th star
        rating.Find(".mud-rating-item:nth-child(5)").Click();

        // Set Comment
        var commentField = cut.FindComponent<MudTextField<string>>();
        await cut.InvokeAsync(() => commentField.Instance.ValueChanged.InvokeAsync("Test Comment"));

        // Submit
        // Find the specific button ensuring verification works
        var submitBtn = cut.FindComponents<MudButton>().First(b => b.Markup.Contains("Trimite recenzia"));
        
        // Ensure enabled
        cut.WaitForState(() => !submitBtn.Instance.Disabled);
        
        await cut.InvokeAsync(() => submitBtn.Find("button").Click());

        // Assert
        _mockReviewService.Verify(x => x.LeaveReviewAsync(matchUserId.ToString(), 5, "Test Comment"), Times.Once);
        _mockSnackbar.Verify(x => x.Add(It.Is<string>(s => s.Contains("succes")), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), null), Times.Once);

        // Card should be removed
        cut.WaitForState(() => !cut.Markup.Contains("Match Person"));
    }
}
