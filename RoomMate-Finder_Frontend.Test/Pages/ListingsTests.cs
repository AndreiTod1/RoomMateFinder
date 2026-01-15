using Bunit;
using Bunit.TestDoubles;
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

public class ListingsTests : BunitContext, IAsyncLifetime
{
    private readonly Mock<IListingService> _mockListingService;
    private readonly Mock<ISnackbar> _mockSnackbar;
    private readonly AuthenticationState _authState;

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    public ListingsTests()
    {
        _mockListingService = new Mock<IListingService>();
        _mockSnackbar = new Mock<ISnackbar>();

        Services.AddMudServices();
        Services.AddSingleton(_mockListingService.Object);
        Services.AddSingleton(_mockSnackbar.Object);
        Services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiBaseUrl"] = "http://localhost:5000"
            })
            .Build());

        // Setup manual auth
        Services.AddAuthorizationCore();
        var claims = new[] 
        { 
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "User"), 
            new Claim(ClaimTypes.Name, "testuser") 
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        _authState = new AuthenticationState(user);

        var mockAuthProvider = new Mock<AuthenticationStateProvider>();
        mockAuthProvider.Setup(x => x.GetAuthenticationStateAsync()).ReturnsAsync(_authState);
        Services.AddSingleton(mockAuthProvider.Object);

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<Listings> RenderComponent()
    {
        return Render<Listings>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));
    }
    
    private void RenderProviders()
    {
        Render<MudPopoverProvider>();
        Render<MudDialogProvider>();
    }

    [Fact]
    public void Listings_Loading_ShowsProgressCircular()
    {
        // Arrange
        var tcs = new TaskCompletionSource<ListingsResponse>();
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>())).Returns(tcs.Task);
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.FindComponents<MudProgressCircular>().Should().NotBeEmpty();

        // Cleanup
        tcs.SetResult(new ListingsResponse(new List<ListingSummaryDto>(), 0, 1, 12));
    }

    [Fact]
    public void Listings_NoListings_ShowsEmptyMessage()
    {
        // Arrange
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(new List<ListingSummaryDto>(), 0, 1, 12));
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("No rooms found");
            cut.Markup.Should().Contain("Try adjusting your filters");
        });
    }

    [Fact]
    public void Listings_WithListings_DisplaysCards()
    {
        // Arrange
        var listings = new List<ListingSummaryDto>
        {
            new ListingSummaryDto(Guid.NewGuid(), Guid.NewGuid(), "Owner", "Nice Room", "City", "Area", 500, DateTime.Today, new List<string> { "WiFi" }, true)
        };
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 12));
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Nice Room");
            cut.Markup.Should().Contain("500");
            cut.FindComponents<MudCard>().Should().NotBeEmpty();
        });
    }

    [Fact]
    public void Listings_HasTitle()
    {
        // Arrange
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(new List<ListingSummaryDto>(), 0, 1, 12));
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Available Rooms");
            cut.Markup.Should().Contain("Find your perfect room");
        });
    }

    [Fact]
    public void Listings_HasFilterButton()
    {
        // Arrange
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(new List<ListingSummaryDto>(), 0, 1, 12));
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Filters");
        });
    }

    [Fact]
    public void Listings_DisplaysListingCount()
    {
        // Arrange
        var listings = new List<ListingSummaryDto>
        {
            new ListingSummaryDto(Guid.NewGuid(), Guid.NewGuid(), "Owner", "Room 1", "City", "Area", 400, DateTime.Today, new List<string>(), true),
            new ListingSummaryDto(Guid.NewGuid(), Guid.NewGuid(), "Owner", "Room 2", "City", "Area", 600, DateTime.Today, new List<string>(), true)
        };
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 2, 1, 12));
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Showing 2 of 2 rooms");
        });
    }

    [Fact]
    public void Listings_ListingCard_HasPriceChip()
    {
        // Arrange
        var listings = new List<ListingSummaryDto>
        {
            new ListingSummaryDto(Guid.NewGuid(), Guid.NewGuid(), "Owner", "Room", "City", "Area", 750, DateTime.Today, new List<string>(), true)
        };
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 12));
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("750");
            cut.Markup.Should().Contain("€/month");
        });
    }

    [Fact]
    public void Listings_ListingCard_ShowsLocation()
    {
        // Arrange
        var listings = new List<ListingSummaryDto>
        {
            new ListingSummaryDto(Guid.NewGuid(), Guid.NewGuid(), "Owner", "Room", "Bucharest", "Sector 1", 500, DateTime.Today, new List<string>(), true)
        };
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 12));
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Bucharest");
        });
    }

    [Fact]
    public async Task Listings_Filter_Apply_ValidatesAndCallsService()
    {
        // Arrange
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(new List<ListingSummaryDto>(), 0, 1, 12));
        
        RenderProviders();
        var cut = RenderComponent();

        // Open filters
        cut.FindComponents<MudButton>().First(b => b.Markup.Contains("Filters")).Find("button").Click();
        cut.WaitForState(() => cut.Markup.Contains("City"));

        // Fill filters
        var cityInput = cut.FindComponents<MudTextField<string>>().First(x => x.Instance.Label == "City").Find("input");
        cityInput.Change("Cluj");

        var minPriceInput = cut.FindComponents<MudNumericField<decimal?>>().First(x => x.Instance.Label == "Min Price (€)").Find("input");
        minPriceInput.Change("200");

        // Apply
        var applyBtn = cut.FindComponents<MudButton>().First(b => b.Markup.Contains("Apply Filters"));
        await cut.InvokeAsync(() => applyBtn.Find("button").Click());

        // Assert
        _mockListingService.Verify(x => x.SearchAsync(It.Is<ListingsSearchRequest>(r => 
            r.City == "Cluj" && r.MinPrice == 200
        )), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Listings_Filter_Clear_ResetsParams()
    {
        // Arrange
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(new List<ListingSummaryDto>(), 0, 1, 12));
        
        RenderProviders();
        var cut = RenderComponent();

        // Open filters
        cut.FindComponents<MudButton>().First(b => b.Markup.Contains("Filters")).Find("button").Click();
        
        // Clear
        var clearBtn = cut.FindComponents<MudButton>().First(b => b.Markup.Contains("Clear"));
        await cut.InvokeAsync(() => clearBtn.Find("button").Click());

        // Assert - verify call with nulls (default)
        _mockListingService.Verify(x => x.SearchAsync(It.Is<ListingsSearchRequest>(r => 
            r.City == null && r.MinPrice == null
        )), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Listings_Pagination_Change_CallsServiceWithNewPage()
    {
        // Arrange
        // Provide at least one listing so the component renders the grid and pagination instead of "No rooms found"
        var listings = new List<ListingSummaryDto>
        {
            new ListingSummaryDto(Guid.NewGuid(), Guid.NewGuid(), "Owner", "Room", "City", "Area", 500, DateTime.Today, new List<string>(), true)
        };
        var total = 20; // 2 pages (12 per page)
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, total, 1, 12));
        
        RenderProviders();
        var cut = RenderComponent();

        // Find page 2 button using aria-label
        var page2Btn = cut.FindAll("button").FirstOrDefault(b => b.GetAttribute("aria-label") == "Go to page 2");
        if (page2Btn == null)
        {
             // Fallback: print all buttons to debug if needed, but for now try finding by text content again strictly
             page2Btn = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Trim() == "2");
        }
        page2Btn.Should().NotBeNull("Page 2 button should exist");
        
        // Act
        await cut.InvokeAsync(() => page2Btn!.Click());

        // Assert
        _mockListingService.Verify(x => x.SearchAsync(It.Is<ListingsSearchRequest>(r => 
            r.Page == 2
        )), Times.Once);
    }

    [Fact]
    public void Listings_Thumbnail_RelativePath_PrependsBaseUrl()
    {
        // Arrange
        var listings = new List<ListingSummaryDto>
        {
            new ListingSummaryDto(Guid.NewGuid(), Guid.NewGuid(), "Owner", "Room", "City", "Area", 500, DateTime.Today, new List<string>(), true, "/uploads/thumb.jpg")
        };
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 12));
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            // BaseUrl configured is http://localhost:5000
            // MudCardMedia uses background-image, not src
            cut.Markup.Should().Contain("http://localhost:5000/uploads/thumb.jpg");
        });
    }

    [Fact]
    public void Listings_ViewRoom_NavigatesToDetails()
    {
        // Arrange
        var id = Guid.NewGuid();
        var listings = new List<ListingSummaryDto>
        {
            new ListingSummaryDto(id, Guid.NewGuid(), "Owner", "Room", "City", "Area", 500, DateTime.Today, new List<string>(), true)
        };
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 12));
        
        RenderProviders();
        var cut = RenderComponent();
        var navMan = Services.GetRequiredService<NavigationManager>();

        // Act
        cut.WaitForAssertion(() => cut.FindComponents<MudCard>().Should().NotBeEmpty());
        cut.Find(".listing-card").Click(); // The card itself has onclick

        // Assert
        navMan.Uri.Should().Contain($"/room/{id}");
    }
}
