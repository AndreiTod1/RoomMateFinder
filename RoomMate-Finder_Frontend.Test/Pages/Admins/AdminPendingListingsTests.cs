using Bunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages.Admins;
using RoomMate_Finder_Frontend.Services;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Pages.Admins;

public class AdminPendingListingsTests : BunitContext, IAsyncLifetime
{
    private readonly Mock<IListingService> _mockListingService;
    private readonly Mock<ISnackbar> _mockSnackbar;
    private readonly Mock<IDialogService> _mockDialogService;

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    public AdminPendingListingsTests()
    {
        _mockListingService = new Mock<IListingService>();
        _mockSnackbar = new Mock<ISnackbar>();
        _mockDialogService = new Mock<IDialogService>();

        _mockSnackbar.Setup(s => s.Configuration).Returns(new SnackbarConfiguration());

        // Setup default search to return empty list
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(new List<ListingSummaryDto>(), 0, 1, 10));

        Services.AddMudServices();
        Services.AddSingleton(_mockListingService.Object);
        Services.AddSingleton(_mockSnackbar.Object);
        Services.AddSingleton(_mockDialogService.Object);
        Services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ApiBaseUrl", "https://api.test.com" }
            }).Build());

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private void RenderProviders()
    {
        Render<MudPopoverProvider>();
        Render<MudDialogProvider>();
    }

    // Component Type Tests
    [Fact]
    public void AdminPendingListings_ComponentExists()
    {
        typeof(AdminPendingListings).Should().NotBeNull();
    }

    [Fact]
    public void AdminPendingListings_HasCorrectPageRoute()
    {
        var pageAttr = typeof(AdminPendingListings).GetCustomAttributes(typeof(Microsoft.AspNetCore.Components.RouteAttribute), false);
        pageAttr.Should().ContainSingle();
        ((Microsoft.AspNetCore.Components.RouteAttribute)pageAttr[0]).Template.Should().Be("/admin/pending-listings");
    }

    [Fact]
    public void AdminPendingListings_HasAuthorizeAttribute()
    {
        var authAttr = typeof(AdminPendingListings).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false);
        authAttr.Should().ContainSingle();
    }

    [Fact]
    public void AdminPendingListings_RequiresAdminRole()
    {
        var authAttr = typeof(AdminPendingListings).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false);
        var attr = authAttr[0] as Microsoft.AspNetCore.Authorization.AuthorizeAttribute;
        attr!.Roles.Should().Be("Admin");
    }

    // Rendering Tests
    [Fact]
    public void AdminPendingListings_Renders_Title()
    {
        RenderProviders();
        var cut = Render<AdminPendingListings>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Pending Listings");
        });
    }

    [Fact]
    public void AdminPendingListings_Renders_Subtitle()
    {
        RenderProviders();
        var cut = Render<AdminPendingListings>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Review and approve or reject");
        });
    }

    [Fact]
    public void AdminPendingListings_HasRefreshButton()
    {
        RenderProviders();
        var cut = Render<AdminPendingListings>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Refresh");
        });
    }

    // Empty State Tests
    [Fact]
    public void AdminPendingListings_NoListings_ShowsEmptyState()
    {
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(new List<ListingSummaryDto>(), 0, 1, 10));

        RenderProviders();
        var cut = Render<AdminPendingListings>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("All Caught Up");
            cut.Markup.Should().Contain("no pending listings");
        });
    }

    [Fact]
    public void AdminPendingListings_NoListings_HasViewAllListingsLink()
    {
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(new List<ListingSummaryDto>(), 0, 1, 10));

        RenderProviders();
        var cut = Render<AdminPendingListings>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("View All Listings");
        });
    }

    // Data Display Tests
    [Fact]
    public void AdminPendingListings_WithListings_DisplaysCount()
    {
        var listings = new List<ListingSummaryDto>
        {
            CreateTestListing(Guid.NewGuid(), "Test Room 1"),
            CreateTestListing(Guid.NewGuid(), "Test Room 2")
        };

        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 2, 1, 10));

        RenderProviders();
        var cut = Render<AdminPendingListings>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("2 listing(s) awaiting review");
        });
    }

    [Fact]
    public void AdminPendingListings_WithListings_ShowsApproveButton()
    {
        var listings = new List<ListingSummaryDto>
        {
            CreateTestListing(Guid.NewGuid(), "Test Room")
        };

        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 10));

        RenderProviders();
        var cut = Render<AdminPendingListings>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Approve");
        });
    }

    [Fact]
    public void AdminPendingListings_WithListings_ShowsRejectButton()
    {
        var listings = new List<ListingSummaryDto>
        {
            CreateTestListing(Guid.NewGuid(), "Test Room")
        };

        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 10));

        RenderProviders();
        var cut = Render<AdminPendingListings>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Reject");
        });
    }

    [Fact]
    public void AdminPendingListings_WithListings_ShowsViewDetailsButton()
    {
        var listings = new List<ListingSummaryDto>
        {
            CreateTestListing(Guid.NewGuid(), "Test Room")
        };

        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 10));

        RenderProviders();
        var cut = Render<AdminPendingListings>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("View Details");
        });
    }

    [Fact]
    public void AdminPendingListings_WithListings_DisplaysTitle()
    {
        var listings = new List<ListingSummaryDto>
        {
            CreateTestListing(Guid.NewGuid(), "My Test Room Title")
        };

        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 10));

        RenderProviders();
        var cut = Render<AdminPendingListings>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("My Test Room Title");
        });
    }

    [Fact]
    public void AdminPendingListings_WithListings_DisplaysPrice()
    {
        var listings = new List<ListingSummaryDto>
        {
            CreateTestListing(Guid.NewGuid(), "Test Room", 500)
        };

        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 10));

        RenderProviders();
        var cut = Render<AdminPendingListings>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("€500");
        });
    }

    [Fact]
    public void AdminPendingListings_WithListings_DisplaysPendingChip()
    {
        var listings = new List<ListingSummaryDto>
        {
            CreateTestListing(Guid.NewGuid(), "Test Room")
        };

        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 10));

        RenderProviders();
        var cut = Render<AdminPendingListings>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("PENDING");
        });
    }

    // Service Registration Tests
    [Fact]
    public void AdminPendingListings_ListingServiceRegistered()
    {
        Services.GetService<IListingService>().Should().NotBeNull();
    }

    [Fact]
    public void AdminPendingListings_SnackbarRegistered()
    {
        Services.GetService<ISnackbar>().Should().NotBeNull();
    }

    [Fact]
    public void AdminPendingListings_DialogServiceRegistered()
    {
        Services.GetService<IDialogService>().Should().NotBeNull();
    }

    [Fact]
    public void AdminPendingListings_ConfigurationRegistered()
    {
        Services.GetService<IConfiguration>().Should().NotBeNull();
    }

    // Helper method - ListingSummaryDto(Guid Id, Guid OwnerId, string OwnerFullName, string Title, string City, string Area, decimal Price, DateTime AvailableFrom, List<string> Amenities, bool IsActive, string? ThumbnailPath, ListingApprovalStatus ApprovalStatus, string? RejectionReason)
    private static ListingSummaryDto CreateTestListing(Guid id, string title, decimal price = 300)
    {
        return new ListingSummaryDto(
            id,
            Guid.NewGuid(),
            "Test Owner",
            title,
            "Test City",
            "Test Area",
            price,
            DateTime.UtcNow.AddMonths(1),
            new List<string> { "WiFi", "Parking" },
            true,
            null,
            ListingApprovalStatus.Pending,
            null
        );
    }
}
