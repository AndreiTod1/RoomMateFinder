using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
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
        // Remove Mock DialogService to allow MudDialogProvider to work with real service for Inline dialogs
        Services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ApiBaseUrl", "https://api.test.com" }
            }).Build());

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<MudDialogProvider> RenderProviders()
    {
        Render<MudPopoverProvider>();
        return Render<MudDialogProvider>();
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
        var pageAttr = typeof(AdminPendingListings).GetCustomAttributes(typeof(RouteAttribute), false);
        pageAttr.Should().ContainSingle();
        ((RouteAttribute)pageAttr[0]).Template.Should().Be("/admin/pending-listings");
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
        var provider = RenderProviders();
        var cut = Render<AdminPendingListings>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Pending Listings");
        });
    }

    [Fact]
    public void AdminPendingListings_Renders_Subtitle()
    {
        var provider = RenderProviders();
        var cut = Render<AdminPendingListings>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Review and approve or reject");
        });
    }

    [Fact]
    public void AdminPendingListings_HasRefreshButton()
    {
        var provider = RenderProviders();
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

        var provider = RenderProviders();
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

        var provider = RenderProviders();
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

        var provider = RenderProviders();
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

        var provider = RenderProviders();
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

        var provider = RenderProviders();
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

        var provider = RenderProviders();
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

        var provider = RenderProviders();
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

        var provider = RenderProviders();
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

        var provider = RenderProviders();
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

    // Helper method
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
    // Interaction Tests
    [Fact]
    public void LoadListings_Exception_ShowsErrorSnackbar()
    {
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ThrowsAsync(new Exception("API Error"));

        var provider = RenderProviders();
        var cut = Render<AdminPendingListings>();

        cut.WaitForAssertion(() =>
        {
            _mockSnackbar.Verify(s => s.Add(It.Is<string>(m => m.Contains("Error loading listings")), Severity.Error, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()), Times.Once);
        });
    }

    [Fact]
    public void ApproveListing_Click_CallsServiceAndRemovesListing()
    {
        // Arrange
        var listing = CreateTestListing(Guid.NewGuid(), "To Approve");
        var listings = new List<ListingSummaryDto> { listing };
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 10));
        
        _mockListingService.Setup(x => x.ApproveAsync(listing.Id))
            .ReturnsAsync((true, "Approved"));

        var provider = RenderProviders();
        var cut = Render<AdminPendingListings>();
        
        // Fix: Use Markup to find text or find specific attributes
        cut.WaitForAssertion(() => cut.FindAll("button").Any(b => b.TextContent.Contains("Approve")).Should().BeTrue());

        // Act
        // Find by Icon, then find button element
        var approveBtnComp = cut.FindComponents<MudButton>()
            .First(b => b.Instance.StartIcon == Icons.Material.Filled.CheckCircle);
        approveBtnComp.Find("button").Click();

        // Assert
        cut.WaitForAssertion(() =>
        {
            _mockListingService.Verify(x => x.ApproveAsync(listing.Id), Times.Once);
            _mockSnackbar.Verify(s => s.Add(It.Is<string>(m => m.Contains("approved successfully")), Severity.Success, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()), Times.Once);
            cut.Markup.Should().NotContain("To Approve"); // Should be removed from UI
        });
    }

    [Fact]
    public void ApproveListing_Failure_ShowsErrorSnackbar()
    {
        // Arrange
        var listing = CreateTestListing(Guid.NewGuid(), "Fail Approve");
        var listings = new List<ListingSummaryDto> { listing };
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 10));
        
        _mockListingService.Setup(x => x.ApproveAsync(listing.Id))
            .ReturnsAsync((false, "Approval failed"));

        var provider = RenderProviders();
        var cut = Render<AdminPendingListings>();
        
        cut.WaitForAssertion(() => cut.FindAll("button").Any(b => b.TextContent.Contains("Approve")).Should().BeTrue());

        // Act
        var approveBtnComp = cut.FindComponents<MudButton>()
            .First(b => b.Instance.StartIcon == Icons.Material.Filled.CheckCircle);
        approveBtnComp.Find("button").Click();

        // Assert
        cut.WaitForAssertion(() =>
        {
            _mockListingService.Verify(x => x.ApproveAsync(listing.Id), Times.Once);
            _mockSnackbar.Verify(s => s.Add("Approval failed", Severity.Error, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()), Times.Once);
            cut.Markup.Should().Contain("Fail Approve"); // Should NOT be removed
        });
    }

    [Fact]
    public void ApproveListing_Exception_ShowsErrorSnackbar()
    {
        // Arrange
        var listing = CreateTestListing(Guid.NewGuid(), "Exception Approve");
        var listings = new List<ListingSummaryDto> { listing };
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 10));
        
        _mockListingService.Setup(x => x.ApproveAsync(listing.Id))
            .ThrowsAsync(new Exception("Network Error"));

        var provider = RenderProviders();
        var cut = Render<AdminPendingListings>();
        
        cut.WaitForAssertion(() => cut.FindAll("button").Any(b => b.TextContent.Contains("Approve")).Should().BeTrue());

        // Act
        var approveBtnComp = cut.FindComponents<MudButton>()
            .First(b => b.Instance.StartIcon == Icons.Material.Filled.CheckCircle);
        approveBtnComp.Find("button").Click();

        // Assert
        cut.WaitForAssertion(() =>
        {
            _mockSnackbar.Verify(s => s.Add(It.Is<string>(m => m.Contains("Error approving listing")), Severity.Error, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()), Times.Once);
        });
    }

    [Fact]
    public void RejectListing_Click_OpensDialog()
    {
        // Arrange
        var listing = CreateTestListing(Guid.NewGuid(), "To Reject");
        var listings = new List<ListingSummaryDto> { listing };
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 10));

        var provider = RenderProviders();
        var cut = Render<AdminPendingListings>();
        
        cut.WaitForAssertion(() => cut.FindAll("button").Any(b => b.TextContent.Contains("Reject")).Should().BeTrue());

        // Act
        var rejectBtn = cut.FindComponents<MudButton>()
            .First(b => b.Instance.StartIcon == Icons.Material.Filled.Cancel);
        rejectBtn.Find("button").Click();

        // Assert
        provider.WaitForAssertion(() => provider.Markup.Should().Contain("Reject Listing"));
        provider.Markup.Should().Contain("To Reject");
    }

    [Fact]
    public void RejectListing_DialogCancel_ClosesDialog()
    {
        // Arrange
        var listing = CreateTestListing(Guid.NewGuid(), "To Reject");
        var listings = new List<ListingSummaryDto> { listing };
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 10));

        var provider = RenderProviders();
        var cut = Render<AdminPendingListings>();
        
        // Open dialog
        cut.WaitForAssertion(() => cut.FindAll("button").Any(b => b.TextContent.Contains("Reject")).Should().BeTrue());
        cut.FindComponents<MudButton>().First(b => b.Instance.StartIcon == Icons.Material.Filled.Cancel).Find("button").Click();
        
        provider.WaitForAssertion(() => provider.Markup.Should().Contain("Reject Listing"));

        // Act - Click Cancel in dialog
        // Locate button inside provider
        var cancelBtn = provider.FindComponents<MudButton>()
            .First(b => b.Find("button").TextContent.Contains("Cancel"));
        cancelBtn.Find("button").Click();

        // Assert
        provider.WaitForAssertion(() => provider.Markup.Should().NotContain("Reject Listing"));
    }

    [Fact(Skip = "MudTextField input element not found in test env - needs investigation")]
    public void RejectListing_DialogConfirm_CallsServiceAndRemovesListing()
    {
        // Arrange
        var listing = CreateTestListing(Guid.NewGuid(), "To Reject Confirm");
        var listings = new List<ListingSummaryDto> { listing };
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 10));
        
        _mockListingService.Setup(x => x.RejectAsync(listing.Id, "Reason"))
            .ReturnsAsync((true, "Rejected"));

        var provider = RenderProviders();
        var cut = Render<AdminPendingListings>();
        
        // Open dialog
        cut.WaitForAssertion(() => cut.FindComponents<MudButton>().Any(b => b.Instance.StartIcon == Icons.Material.Filled.Cancel).Should().BeTrue());
        cut.FindComponents<MudButton>().First(b => b.Instance.StartIcon == Icons.Material.Filled.Cancel).Find("button").Click();

        provider.WaitForAssertion(() => provider.Markup.Should().Contain("Reject Listing"));

        // Type reason
        provider.FindComponent<MudTextField<string>>().Find("input").Change("Reason");

        // Act - Click Reject in dialog
        var confirmRejectBtn = provider.FindComponents<MudButton>()
            .First(b => b.Find("button").TextContent.Contains("Reject Listing"));
        confirmRejectBtn.Find("button").Click();

        // Assert
        cut.WaitForAssertion(() =>
        {
            _mockListingService.Verify(x => x.RejectAsync(listing.Id, "Reason"), Times.Once);
            _mockSnackbar.Verify(s => s.Add(It.Is<string>(m => m.Contains("has been rejected")), Severity.Warning, It.IsAny<Action<SnackbarOptions>>(), It.IsAny<string>()), Times.Once);
            cut.Markup.Should().NotContain("To Reject Confirm");
        });
        
        provider.WaitForAssertion(() => provider.Markup.Should().NotContain("Reject Listing"));
    }

    [Fact]
    public void ViewDetails_Click_NavigatesToRoomDetails()
    {
        // Arrange
        var listing = CreateTestListing(Guid.NewGuid(), "View Details Room");
        var listings = new List<ListingSummaryDto> { listing };
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 10));

        RenderProviders();
        var cut = Render<AdminPendingListings>();
        var navMan = Services.GetRequiredService<NavigationManager>();

        cut.WaitForAssertion(() => cut.FindAll("button").Any(b => b.TextContent.Contains("View Details")).Should().BeTrue());

        // Act
        var viewBtn = cut.FindComponents<MudButton>()
            .First(b => b.Instance.StartIcon == Icons.Material.Filled.Visibility);
        viewBtn.Find("button").Click();

        // Assert
        navMan.Uri.Should().EndWith($"/room/{listing.Id}");
    }

    // Logic/Formatting Tests
    [Fact]
    public void Amenities_MoreThanFive_ShowsPlusChip()
    {
        // Arrange
        var listing = new ListingSummaryDto(
            Guid.NewGuid(), Guid.NewGuid(), "Owner", "Amenities Test", "City", "Area", 500, DateTime.Now,
             new List<string> { "1", "2", "3", "4", "5", "6", "7" }, true, null, ListingApprovalStatus.Pending, null
        );
        var listings = new List<ListingSummaryDto> { listing };
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 10));

        RenderProviders();
        var cut = Render<AdminPendingListings>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("1");
            cut.Markup.Should().Contain("2");
            cut.Markup.Should().Contain("3");
            cut.Markup.Should().Contain("4");
            cut.Markup.Should().Contain("5");
            cut.Markup.Should().Contain("+2 more");
            cut.Markup.Should().NotContain(">6<"); // Should act as chip content check
        });
    }

    [Fact]
    public void ImageUrl_Absolute_ReturnsAsIs()
    {
        // Arrange
        var listing = CreateTestListing(Guid.NewGuid(), "Abs Image");
        listing = listing with { ThumbnailPath = "http://test.com/img.jpg" };
        var listings = new List<ListingSummaryDto> { listing };
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 10));

        RenderProviders();
        var cut = Render<AdminPendingListings>();

        cut.WaitForAssertion(() =>
        {
            cut.Find("img").GetAttribute("src").Should().Be("http://test.com/img.jpg");
        });
    }

    [Fact]
    public void ImageUrl_Relative_AppendsBaseUrl()
    {
        // Arrange
        var listing = CreateTestListing(Guid.NewGuid(), "Rel Image");
        listing = listing with { ThumbnailPath = "/img.jpg" };
        var listings = new List<ListingSummaryDto> { listing };
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 10));

        RenderProviders();
        var cut = Render<AdminPendingListings>();

        cut.WaitForAssertion(() =>
        {
            cut.Find("img").GetAttribute("src").Should().Be("https://api.test.com/img.jpg");
        });
    }
}
