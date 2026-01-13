using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using Microsoft.Extensions.Configuration;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class MyListingsTests : TestContext, IAsyncLifetime
{
    private readonly Mock<IListingService> _mockListingService;
    private readonly Mock<AuthenticationStateProvider> _mockAuthStateProvider;
    private readonly Guid _testUserId = Guid.NewGuid();

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    public MyListingsTests()
    {
        _mockListingService = new Mock<IListingService>();
        _mockAuthStateProvider = new Mock<AuthenticationStateProvider>();

        Services.AddMudServices();
        Services.AddSingleton(_mockListingService.Object);
        Services.AddSingleton(_mockAuthStateProvider.Object);
        Services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        
        // Mock authenticated user
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Email, "test@test.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var authState = Task.FromResult(new AuthenticationState(claimsPrincipal));
        
        _mockAuthStateProvider.Setup(x => x.GetAuthenticationStateAsync())
            .Returns(authState);

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private void RenderProviders()
    {
        Render<MudPopoverProvider>();
        Render<MudDialogProvider>();
        Render<MudSnackbarProvider>();
    }

    [Fact]
    public async Task MyListings_Loading_ShowsProgressIndicator()
    {
        //Arrange
        var tcs = new TaskCompletionSource<ListingsResponse>();
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .Returns(tcs.Task);

        RenderProviders();

        // Act
        var cut = Render<MyListings>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Loading your listings");
        });

        // Cleanup
        tcs.SetResult(new ListingsResponse(new List<ListingSummaryDto>(), 0, 1, 100));
    }

    [Fact]
    public void MyListings_NoListings_ShowsEmptyState()
    {
        // Arrange
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(new List<ListingSummaryDto>(), 0, 1, 100));

        RenderProviders();

        // Act
        var cut = Render<MyListings>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("No Listings Yet");
        });
    }

    [Fact]
    public void MyListings_WithPendingListings_DisplaysPendingSection()
    {
        // Arrange
        var listings = new List<ListingSummaryDto>
        {
            new ListingSummaryDto(
                Id: Guid.NewGuid(),
                OwnerId: _testUserId,
                OwnerFullName: "Owner Name",
                Title: "Pending Room",
                City: "Cluj-Napoca",
                Area: "Centru",
                Price: 500,
                AvailableFrom: DateTime.UtcNow.AddMonths(1),
                Amenities: new List<string>(),
                IsActive: true,
                ThumbnailPath: null,
                ApprovalStatus: ListingApprovalStatus.Pending,
                RejectionReason: null
            )
        };

        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 100));

        RenderProviders();

        // Act
        var cut = Render<MyListings>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Pending Room");
            cut.Markup.Should().Contain("PENDING");
        });
    }

    [Fact]
    public void MyListings_WithApprovedListings_DisplaysApprovedSection()
    {
        // Arrange
        var listings = new List<ListingSummaryDto>
        {
            new ListingSummaryDto(
                Id: Guid.NewGuid(),
                OwnerId: _testUserId,
                OwnerFullName: "Owner Name",
                Title: "Approved Room",
                City: "Cluj-Napoca",
                Area: "Centru",
                Price: 600,
                AvailableFrom: DateTime.UtcNow.AddMonths(1),
                Amenities: new List<string>(),
                IsActive: true,
                ThumbnailPath: null,
                ApprovalStatus: ListingApprovalStatus.Approved,
                RejectionReason: null
            )
        };

        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 100));

        RenderProviders();

        // Act
        var cut = Render<MyListings>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Approved Room");
            cut.Markup.Should().Contain("APPROVED");
        });
    }

    [Fact]
    public void MyListings_WithRejectedListings_DisplaysRejectedSection()
    {
        // Arrange
        var listings = new List<ListingSummaryDto>
        {
            new ListingSummaryDto(
                Id: Guid.NewGuid(),
                OwnerId: _testUserId,
                OwnerFullName: "Owner Name",
                Title: "Rejected Room",
                City: "Cluj-Napoca",
                Area: "Centru",
                Price: 400,
                AvailableFrom: DateTime.UtcNow.AddMonths(1),
                Amenities: new List<string>(),
                IsActive: false,
                ThumbnailPath: null,
                ApprovalStatus: ListingApprovalStatus.Rejected,
                RejectionReason: "Incomplete information"
            )
        };

        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(listings, 1, 1, 100));

        RenderProviders();

        // Act
        var cut = Render<MyListings>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Rejected Room");
            cut.Markup.Should().Contain("REJECTED");
            cut.Markup.Should().Contain("Incomplete information");
        });
    }
}
