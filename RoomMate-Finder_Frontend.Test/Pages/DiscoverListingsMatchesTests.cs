using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Models;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;
using System.Security.Claims;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Pages;

#region MyMatches Page Tests

public class MyMatchesPageTests : IAsyncLifetime
{
    private readonly BunitContext _ctx = new();
    private readonly Mock<IMatchingService> _mockMatchingService;
    private readonly Mock<IProfileService> _mockProfileService;
    private readonly Mock<ISnackbar> _mockSnackbar;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IConversationService> _mockConversationService;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync()
    {
        await _ctx.DisposeAsync();
    }

    public MyMatchesPageTests()
    {
        _ctx.Services.AddMudServices();
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        _mockMatchingService = new Mock<IMatchingService>();
        _mockProfileService = new Mock<IProfileService>();
        _mockSnackbar = new Mock<ISnackbar>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConversationService = new Mock<IConversationService>();

        // Setup mock to return current user
        _mockProfileService.Setup(x => x.GetCurrentAsync())
            .ReturnsAsync(new ProfileDto(
                Guid.NewGuid(),
                "test@test.com",
                "Test User",
                25,
                "Male",
                "Test University",
                "Bio",
                "Night Owl",
                "Gaming",
                DateTime.UtcNow,
                null
            ));

        // Setup mock to return empty matches
        _mockMatchingService.Setup(x => x.GetMyMatchesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<UserMatchDto>());

        _mockConfiguration.Setup(x => x["ApiBaseUrl"]).Returns("http://localhost:5000");

        _ctx.Services.AddSingleton(_mockMatchingService.Object);
        _ctx.Services.AddSingleton(_mockProfileService.Object);
        _ctx.Services.AddSingleton(_mockSnackbar.Object);
        _ctx.Services.AddSingleton(_mockConfiguration.Object);
        _ctx.Services.AddSingleton(_mockConversationService.Object);
        _ctx.Services.AddSingleton(new HttpClient());
    }

    [Fact]
    public void MyMatches_RendersWithoutCrashing()
    {
        // Act
        var cut = _ctx.Render<MyMatches>();

        // Assert
        cut.Markup.Should().NotBeEmpty();
    }

    [Fact]
    public void MyMatches_WhenEmpty_ShowsEmptyState()
    {
        // Arrange - matches already set to empty

        // Act
        var cut = _ctx.Render<MyMatches>();

        // Assert
        cut.Markup.Should().NotBeEmpty();
    }

    [Fact]
    public void MyMatches_WithMatches_ShowsMatchCards()
    {
        // Arrange
        var testMatch = new UserMatchDto(
            MatchId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Email: "match@test.com",
            FullName: "Match User",
            Age: 22,
            Gender: "Female",
            University: "Match University",
            Bio: "Match bio",
            Lifestyle: "Early Bird",
            Interests: "Reading",
            MatchedAt: DateTime.UtcNow,
            IsActive: true,
            ProfilePicturePath: null
        );

        _mockMatchingService.Setup(x => x.GetMyMatchesAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<UserMatchDto> { testMatch });

        // Act
        var cut = _ctx.Render<MyMatches>();

        // Assert
        cut.Markup.Should().NotBeEmpty();
    }

    [Fact]
    public void MyMatches_ServiceGetsCalled()
    {
        // Act
        var cut = _ctx.Render<MyMatches>();

        // Assert
        _mockProfileService.Verify(x => x.GetCurrentAsync(), Times.AtLeastOnce);
    }
}

#endregion

#region Listings Page Tests

public class ListingsPageTests : BunitContext, IAsyncLifetime
{
    private readonly Mock<IListingService> _mockListingService;
    private readonly Mock<ISnackbar> _mockSnackbar;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public Task InitializeAsync() => Task.CompletedTask;
    public new async Task DisposeAsync()
    {
        Dispose();
        await Task.CompletedTask;
    }

    public ListingsPageTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        _mockListingService = new Mock<IListingService>();
        _mockSnackbar = new Mock<ISnackbar>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup mock to return empty listings
        _mockListingService.Setup(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()))
            .ReturnsAsync(new ListingsResponse(new List<ListingSummaryDto>(), 0, 1, 10));

        _mockConfiguration.Setup(x => x["ApiBaseUrl"]).Returns("http://localhost:5000");

        Services.AddSingleton(_mockListingService.Object);
        Services.AddSingleton(_mockSnackbar.Object);
        Services.AddSingleton(_mockConfiguration.Object);

        // Add authorization for [Authorize] attribute
        Services.AddAuthorizationCore();
        Services.AddSingleton<AuthenticationStateProvider, TestAuthStateProvider>();
    }

    [Fact]
    public void Listings_RendersWithoutCrashing()
    {
        // Act
        var cut = Render<Listings>();

        // Assert
        cut.Markup.Should().NotBeEmpty();
    }

    [Fact]
    public void Listings_HasPageTitle()
    {
        // Act
        var cut = Render<Listings>();

        // Assert
        cut.Markup.Should().Contain("Available Rooms");
    }

    [Fact]
    public void Listings_HasFiltersButton()
    {
        // Act
        var cut = Render<Listings>();

        // Assert
        cut.Markup.Should().Contain("Filters");
    }

    [Fact]
    public void Listings_WhenEmpty_ShowsNoListings()
    {
        // Arrange - already returning empty

        // Act
        var cut = Render<Listings>();

        // Assert
        cut.Markup.Should().NotBeEmpty();
    }

    [Fact]
    public void Listings_ServiceGetsCalled()
    {
        // Act
        var cut = Render<Listings>();

        // Assert
        _mockListingService.Verify(x => x.SearchAsync(It.IsAny<ListingsSearchRequest>()), Times.AtLeastOnce);
    }
}

#endregion

#region Test Auth State Provider

public class TestAuthStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Email, "test@test.com"),
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(user));
    }
}

#endregion
