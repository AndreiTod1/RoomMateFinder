using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;
using System.Security.Claims;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class RoomDetailsTests : BunitContext, IAsyncLifetime
{
    private readonly Mock<IListingService> _mockListingService;
    private readonly Mock<IConversationService> _mockConversationService;
    private readonly Mock<ISnackbar> _mockSnackbar;

    public Task InitializeAsync() => Task.CompletedTask;
    public new async Task DisposeAsync() => await base.DisposeAsync();

    public RoomDetailsTests()
    {
        Services.AddMudServices();
        
        // Setup JSInterop for MudBlazor components
        JSInterop.Mode = JSRuntimeMode.Loose;
        
        _mockListingService = new Mock<IListingService>();
        _mockConversationService = new Mock<IConversationService>();
        _mockSnackbar = new Mock<ISnackbar>();

        Services.AddSingleton(_mockListingService.Object);
        Services.AddSingleton(_mockConversationService.Object);
        Services.AddSingleton(_mockSnackbar.Object);
        Services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ApiBaseUrl", "http://test-api.com" }
            }).Build());

        // Simple auth without custom provider to avoid dispose issues
        Services.AddAuthorizationCore();
        Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthStateProvider());
    }

    private class FakeAuthStateProvider : AuthenticationStateProvider
    {
        public ClaimsPrincipal User { get; set; } = new ClaimsPrincipal(new ClaimsIdentity());

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(User));
        }
    }

    private static ListingDto CreateTestListing(Guid id, Guid ownerId)
    {
        return new ListingDto(
            id, ownerId, "Owner Name", "Test Room", "City", "Area", 500, DateTime.Now,
            new List<string> { "http://test-api.com/img1.jpg", "http://test-api.com/img2.jpg" }, DateTime.Now, true,
            new List<string> { "Wifi", "AC" }, "Description"
        );
    }

    [Fact]
    public void RoomDetails_NotFound_ShowsErrorMessage()
    {
        _mockListingService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((ListingDto?)null);

        var cut = Render<RoomDetails>(p => p.Add(x => x.Id, Guid.NewGuid()));

        cut.Markup.Should().Contain("Room not found");
    }

    [Fact]
    public void RoomDetails_Exception_ShowsSnackbar()
    {
        _mockListingService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception("API Fail"));

        var cut = Render<RoomDetails>(p => p.Add(x => x.Id, Guid.NewGuid()));

        _mockSnackbar.Verify(x => x.Add(It.Is<string>(s => s.Contains("Error loading")), Severity.Error, null, null), Times.Once);
    }

    [Fact]
    public void RoomDetails_Found_RendersDetails()
    {
        var listingId = Guid.NewGuid();
        var listing = CreateTestListing(listingId, Guid.NewGuid());
        
        _mockListingService.Setup(x => x.GetByIdAsync(listingId))
            .ReturnsAsync(listing);

        var cut = Render<RoomDetails>(p => p.Add(x => x.Id, listingId));
        
        cut.WaitForState(() => cut.FindAll("h4").Any()); // Wait for load

        cut.Markup.Should().Contain("Test Room");
        cut.Markup.Should().Contain("500");
        cut.Markup.Should().Contain("Description");
        cut.Markup.Should().Contain("Owner Name");
    }
    
    [Fact]
    public void RoomDetails_BackNavigation_Works()
    {
        var listingId = Guid.NewGuid();
        _mockListingService.Setup(x => x.GetByIdAsync(listingId))
           .ReturnsAsync(CreateTestListing(listingId, Guid.NewGuid()));
           
        var cut = Render<RoomDetails>(p => p.Add(x => x.Id, listingId));
        cut.WaitForState(() => cut.FindAll("h4").Any());

        var navMan = Services.GetRequiredService<NavigationManager>();
        
        // Find Back button
        cut.Find("button").Click(); 
        
        navMan.Uri.Should().EndWith("/listings");
    }
    
    [Fact]
    public void RoomDetails_ContactOwner_NotLoggedIn_ShowsWarning()
    {
        var listingId = Guid.NewGuid();
        _mockListingService.Setup(x => x.GetByIdAsync(listingId))
            .ReturnsAsync(CreateTestListing(listingId, Guid.NewGuid()));

        var cut = Render<RoomDetails>(p => p.Add(x => x.Id, listingId));
        cut.WaitForState(() => cut.FindAll("h4").Any());

        // Not logged in by default in FakeAuthStateProvider

        // Click Contact Owner
        var contactBtn = cut.FindComponents<MudButton>()
            .First(b => b.Markup.Contains("Contact Owner"));
        contactBtn.Find("button").Click();

        _mockSnackbar.Verify(s => s.Add(It.Is<string>(m => m.Contains("Please log in")), Severity.Warning, null, null), Times.Once);
        
        var navMan = Services.GetRequiredService<NavigationManager>();
        navMan.Uri.Should().EndWith("/login");
    }
}
