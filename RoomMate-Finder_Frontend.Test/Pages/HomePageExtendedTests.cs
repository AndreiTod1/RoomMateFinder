using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class HomePageExtendedTests : BunitContext
{
    public HomePageExtendedTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Home_ShouldRenderMainContent()
    {
        // Act
        var cut = Render<Home>();

        // Assert
        cut.Markup.Should().NotBeEmpty();
    }

    [Fact]
    public void Home_ShouldContainWelcomeText()
    {
        // Act
        var cut = Render<Home>();

        // Assert
        cut.Markup.Should().Contain("Găsește-ți colegul");
    }

    [Fact]
    public void Home_ShouldHaveRegisterButton()
    {
        // Act
        var cut = Render<Home>();

        // Assert
        var links = cut.FindAll("a");
        var hasRegisterLink = links.Any(e =>
        {
            var href = e.GetAttribute("href");
            return href != null && href.Contains("register");
        });
        hasRegisterLink.Should().BeTrue("Home page should have a register link");
    }

    [Fact]
    public void Home_ShouldHaveLoginButton()
    {
        // Act
        var cut = Render<Home>();

        // Assert
        var links = cut.FindAll("a");
        var hasLoginLink = links.Any(e =>
        {
            var href = e.GetAttribute("href");
            return href != null && href.Contains("login");
        });
        hasLoginLink.Should().BeTrue("Home page should have a login link");
    }

    [Fact]
    public void Home_ShouldSetPageTitle()
    {
        // Act
        var cut = Render<Home>();

        // Assert - Check PageTitle component exists
        var pageTitle = cut.FindComponents<Microsoft.AspNetCore.Components.Web.PageTitle>();
        pageTitle.Should().NotBeEmpty();
    }

    [Fact]
    public void Home_ShouldContainDescriptionText()
    {
        // Act
        var cut = Render<Home>();

        // Assert - Check for some descriptive content
        cut.Markup.Should().Contain("coleg");
    }
}

public class ContactPageExtendedTests : BunitContext
{
    private readonly Mock<IProfileService> _mockProfileService;
    private readonly Mock<ISnackbar> _mockSnackbar;

    public ContactPageExtendedTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        
        _mockProfileService = new Mock<IProfileService>();
        _mockSnackbar = new Mock<ISnackbar>();
        
        _mockProfileService.Setup(x => x.GetAdminsAsync())
            .ReturnsAsync(new List<ProfileDto>());
        
        Services.AddSingleton(_mockProfileService.Object);
        Services.AddSingleton(_mockSnackbar.Object);
    }

    [Fact]
    public void Contact_ShouldRenderPageTitle()
    {
        // Act
        var cut = Render<Contact>();

        // Assert - Check page renders without error
        cut.Markup.Should().NotBeEmpty();
    }
}

