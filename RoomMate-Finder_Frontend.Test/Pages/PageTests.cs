using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class HomePageTests : BunitContext, IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;
    public new Task DisposeAsync()
    {
        Dispose();
        return Task.CompletedTask;
    }

    public HomePageTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Home_RendersMainTitle()
    {
        // Act
        var cut = Render<Home>();

        // Assert
        cut.Markup.Should().Contain("Găsește-ți colegul");
    }

    [Fact]
    public void Home_HasRegisterLink()
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
        hasRegisterLink.Should().BeTrue();
    }

    [Fact]
    public void Home_HasLoginLink()
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
        hasLoginLink.Should().BeTrue();
    }

    [Fact]
    public void Home_ContainsContent()
    {
        // Act
        var cut = Render<Home>();

        // Assert
        cut.Markup.Should().NotBeEmpty();
    }
}

public class ContactPageTests : BunitContext, IAsyncLifetime
{
    private readonly Mock<IProfileService> _mockProfileService;
    private readonly Mock<ISnackbar> _mockSnackbar;

    public Task InitializeAsync() => Task.CompletedTask;
    public new Task DisposeAsync()
    {
        Dispose();
        return Task.CompletedTask;
    }

    public ContactPageTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        
        _mockProfileService = new Mock<IProfileService>();
        _mockSnackbar = new Mock<ISnackbar>();
        
        // Setup mock to return empty list of admins
        _mockProfileService.Setup(x => x.GetAdminsAsync())
            .ReturnsAsync(new List<ProfileDto>());
        
        Services.AddSingleton(_mockProfileService.Object);
        Services.AddSingleton(_mockSnackbar.Object);
        Services.AddSingleton(new HttpClient());
    }

    [Fact]
    public void Contact_RendersCorrectly()
    {
        // Act
        var cut = Render<Contact>();

        // Assert
        cut.Markup.Should().NotBeEmpty();
    }

    [Fact]
    public void Contact_HasContactInformation()
    {
        // Act
        var cut = Render<Contact>();

        // Assert
        cut.Markup.Should().NotBeEmpty();
    }
}

