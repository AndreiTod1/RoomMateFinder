using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization; // For AuthenticationStateProvider
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;
using RoomMate_Finder_Frontend.Shared;
using System.Security.Claims;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class MyListingsTests : BunitContext, IAsyncLifetime
{
    private readonly Mock<IListingService> _mockListingService;
    private readonly Mock<ISnackbar> _mockSnackbar;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly Mock<IConfiguration> _mockConfig;

    public Task InitializeAsync() => Task.CompletedTask;
    public new async Task DisposeAsync() => await base.DisposeAsync();

    public MyListingsTests()
    {
        Services.AddMudServices();
        
        // Setup JSInterop for MudBlazor components
        JSInterop.Mode = JSRuntimeMode.Loose;
        
        _mockListingService = new Mock<IListingService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockSnackbar = new Mock<ISnackbar>();
        _mockConfig = new Mock<IConfiguration>();
        _mockConfig.Setup(c => c["ApiBaseUrl"]).Returns("http://test-api.com");

        Services.AddSingleton(_mockListingService.Object);
        Services.AddSingleton(_mockConfig.Object);
        
        // Manual Auth setup - using Singleton to avoid Dispose issues
        Services.AddAuthorizationCore();
        Services.AddSingleton<AuthenticationStateProvider>(new TestAuthStateProvider());
    }
    
    // Simple Test Auth Provider
    class TestAuthStateProvider : AuthenticationStateProvider
    {
        public ClaimsPrincipal User { get; set; } = new ClaimsPrincipal(new ClaimsIdentity());

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(User));
        }

        public void SetUser(string name, Guid id)
        {
            var claims = new[] 
            { 
                 new Claim(ClaimTypes.Name, name),
                 new Claim(ClaimTypes.NameIdentifier, id.ToString())
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            User = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }

    [Fact]
    public void MyListings_Renders_TitleAndCreateButton()
    {
        var authProvider = (TestAuthStateProvider)Services.GetRequiredService<AuthenticationStateProvider>();
        authProvider.SetUser("Test User", Guid.NewGuid());

        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(new List<ListingSummaryDto>(), 0, 1, 100));

        var cut = Render<MyListings>();

        cut.Markup.Should().Contain("My Listings");
        cut.Markup.Should().Contain("Submit New Listing");
    }

    [Fact]
    public void MyListings_NoListings_ShowsEmptyMessage()
    {
        var authProvider = (TestAuthStateProvider)Services.GetRequiredService<AuthenticationStateProvider>();
        authProvider.SetUser("Test User", Guid.NewGuid());

        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(new List<ListingSummaryDto>(), 0, 1, 100));

        var cut = Render<MyListings>();

        cut.WaitForAssertion(() => cut.Markup.Should().Contain("No Listings Yet"));
    }

    [Fact]
    public void MyListings_WithListings_RendersCards()
    {
        var userId = Guid.NewGuid();
        var authProvider = (TestAuthStateProvider)Services.GetRequiredService<AuthenticationStateProvider>();
        authProvider.SetUser("Test User", userId);

        var listings = new List<ListingSummaryDto>
        {
            new ListingSummaryDto(Guid.NewGuid(), userId, "Owner", "My Room", "City", "Area", 400, DateTime.Now, new List<string>(), true, null, ListingApprovalStatus.Approved, null)
        };
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 100));

        var cut = Render<MyListings>();

        cut.WaitForAssertion(() => cut.Markup.Should().Contain("My Room"));
        cut.Markup.Should().Contain("City");
        cut.Markup.Should().Contain("Area");
        cut.Markup.Should().Contain("400");
    }
}
