using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;
using Xunit;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class RoomDetailsTests : IAsyncLifetime
{
    private readonly TestContext _ctx = new();
    private readonly Mock<IListingService> _mockListingService;
    private readonly Mock<IConversationService> _mockConversationService;
    private readonly Mock<NavigationManager> _mockNav;
    private readonly Mock<AuthenticationStateProvider> _mockAuthProvider;
    
    public RoomDetailsTests()
    {
        _ctx.Services.AddMudServices();
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        _mockListingService = new Mock<IListingService>();
        _mockConversationService = new Mock<IConversationService>();
        _mockAuthProvider = new Mock<AuthenticationStateProvider>();
        
        // Mock Configuration
        var inMemorySettings = new Dictionary<string, string> {
            {"ApiBaseUrl", "http://localhost:5000/"}
        };
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _ctx.Services.AddSingleton(_mockListingService.Object);
        _ctx.Services.AddSingleton(_mockConversationService.Object);
        _ctx.Services.AddSingleton(_mockAuthProvider.Object); // Manual Register
        _ctx.Services.AddSingleton(configuration);
        
        // Default Auth State (Unauthenticated)
        _mockAuthProvider.Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _ctx.DisposeAsync();
    }

    [Fact]
    public void RoomDetails_LoadingState_RendersSpinner()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        _mockListingService.Setup(x => x.GetByIdAsync(listingId))
            .Returns(async () => { await Task.Delay(100); return null; }); 

        // Act
        var cut = _ctx.Render<MudPopoverProvider>();
        var page = _ctx.Render<RoomDetails>(parameters => parameters.Add(p => p.Id, listingId));

        // Assert
        page.FindComponents<MudProgressCircular>().Should().NotBeEmpty();
        page.Markup.Should().Contain("Loading room details");
    }

    [Fact]
    public void RoomDetails_NotFound_RendersError()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        _mockListingService.Setup(x => x.GetByIdAsync(listingId))
            .ReturnsAsync((ListingDto?)null);

        // Act
        var cut = _ctx.Render<MudPopoverProvider>();
        var page = _ctx.Render<RoomDetails>(parameters => parameters.Add(p => p.Id, listingId));

        // Assert
        page.WaitForState(() => page.Markup.Contains("Room not found"));
        page.Markup.Should().Contain("Room not found");
    }

    [Fact]
    public void RoomDetails_Success_RendersDetails()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        var listing = new ListingDto(
            listingId,
            Guid.NewGuid(),
            "Nice Room",
            "A very nice room",
            "Cluj",
            "Center",
            400,
            DateTime.UtcNow,
            new List<string> { "Wifi", "AC" },
            DateTime.UtcNow,
            true,
            new List<string> { "/img1.jpg", "/img2.jpg" },
            "Owner Name"
        );

        _mockListingService.Setup(x => x.GetByIdAsync(listingId)).ReturnsAsync(listing);

        // Act
        var cut = _ctx.Render<MudPopoverProvider>();
        var page = _ctx.Render<RoomDetails>(parameters => parameters.Add(p => p.Id, listingId));

        // Assert
        page.WaitForState(() => page.FindAll("h4").Any(e => e.TextContent.Contains("Nice Room")));
        
        page.FindAll(".mud-chip").Any(c => c.TextContent.Contains("Wifi")).Should().BeTrue();
        page.FindAll(".mud-chip").Any(c => c.TextContent.Contains("400")).Should().BeTrue();
        page.Markup.Should().Contain("Cluj");
        page.Markup.Should().Contain("Owner Name");
    }

    [Fact]
    public void RoomDetails_ContactOwner_Unauthenticated_ShowsWarning()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        var listing = new ListingDto(
            listingId,
            Guid.NewGuid(),
            "Nice Room",
            "Desc",
            "City",
            "Area",
            400,
            DateTime.UtcNow,
            new List<string>(),
            DateTime.UtcNow,
            true,
            null,
            "Owner"
        );

        _mockListingService.Setup(x => x.GetByIdAsync(listingId)).ReturnsAsync(listing);
        
        // Ensure unauthenticated (default)
        // No action needed as setup in constructor
        
        var cut = _ctx.Render<MudPopoverProvider>();
        var page = _ctx.Render<RoomDetails>(parameters => parameters.Add(p => p.Id, listingId));
        
        page.WaitForState(() => page.FindAll("button").Any(b => b.TextContent.Contains("Contact Owner")));

        // Act
        var btnElement = page.Find("button.mud-button-filled.mud-button-filled-primary");
        btnElement.Click();

        // Assert
        var navMan = _ctx.Services.GetRequiredService<NavigationManager>();
        Assert.Equal("http://localhost/login", navMan.Uri);
    }

    [Fact]
    public void RoomDetails_ContactOwner_Authenticated_StartsConversation()
    {
       // Arrange
        var listingId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        
        var listing = new ListingDto(
            listingId,
            ownerId,
            "Nice Room",
            "Desc",
            "City",
            "Area",
            400,
            DateTime.UtcNow,
            new List<string>(),
            DateTime.UtcNow,
            true,
            null,
            "Owner"
        );

        _mockListingService.Setup(x => x.GetByIdAsync(listingId)).ReturnsAsync(listing);
        
        // Mock Auth
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, currentUserId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _mockAuthProvider.Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(claimsPrincipal));
            
        // Mock Conversation
        _mockConversationService.Setup(x => x.StartConversationAsync(ownerId))
            .ReturnsAsync(new ConversationDto(Guid.NewGuid(), ownerId, "Owner Name", null, "User", DateTime.UtcNow)); 

        var cut = _ctx.Render<MudPopoverProvider>();
        var page = _ctx.Render<RoomDetails>(parameters => parameters.Add(p => p.Id, listingId));
        
        page.WaitForState(() => page.FindAll("button").Any(b => b.TextContent.Contains("Contact Owner")));

        // Act
        var btnElement = page.Find("button.mud-button-filled.mud-button-filled-primary");
        btnElement.Click();

        // Assert
        _mockConversationService.Verify(x => x.StartConversationAsync(ownerId), Times.Once);
        // Should navigate to messages
        var navMan = _ctx.Services.GetRequiredService<NavigationManager>();
        Assert.Equal("http://localhost/messages", navMan.Uri);
    }
}
