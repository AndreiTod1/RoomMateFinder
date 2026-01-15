using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;
using System.Net.Http;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class ContactTests : BunitContext, IAsyncLifetime
{
    private readonly Mock<IProfileService> _mockProfileService;
    private readonly Mock<ISnackbar> _mockSnackbar;

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    public ContactTests()
    {
        _mockProfileService = new Mock<IProfileService>();
        _mockSnackbar = new Mock<ISnackbar>();

        Services.AddMudServices();
        Services.AddSingleton(_mockProfileService.Object);
        Services.AddSingleton(_mockSnackbar.Object);
        Services.AddSingleton(new HttpClient { BaseAddress = new Uri("http://localhost:5000") });

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<Contact> RenderComponent()
    {
        return Render<Contact>();
    }
    
    private void RenderProviders()
    {
        Render<MudPopoverProvider>();
        Render<MudDialogProvider>();
    }

    [Fact]
    public void Contact_Loading_ShowsProgressCircular()
    {
        // Arrange
        var tcs = new TaskCompletionSource<List<ProfileDto>>();
        _mockProfileService.Setup(x => x.GetAdminsAsync()).Returns(tcs.Task);
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.FindComponents<MudProgressCircular>().Should().NotBeEmpty();

        // Cleanup
        tcs.SetResult(new List<ProfileDto>());
    }

    [Fact]
    public void Contact_NoAdmins_ShowsEmptyMessage()
    {
        // Arrange
        _mockProfileService.Setup(x => x.GetAdminsAsync()).ReturnsAsync(new List<ProfileDto>());
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Momentan nu exista administratori disponibili");
        });
    }

    [Fact]
    public void Contact_WithAdmins_DisplaysAdminCards()
    {
        // Arrange
        var admins = new List<ProfileDto>
        {
            new ProfileDto(Guid.NewGuid(), "admin@test.com", "Admin User", 30, "M", "Uni", "Bio", "Style", "Interests", DateTime.UtcNow, null, "Admin")
        };
        _mockProfileService.Setup(x => x.GetAdminsAsync()).ReturnsAsync(admins);
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Admin User");
            cut.Markup.Should().Contain("admin@test.com");
            cut.Markup.Should().Contain("ADMIN");
            cut.Markup.Should().Contain("Trimite email");
        });
    }

    [Fact]
    public void Contact_HasTitle()
    {
        // Arrange
        _mockProfileService.Setup(x => x.GetAdminsAsync()).ReturnsAsync(new List<ProfileDto>());
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Contact");
            cut.Markup.Should().Contain("Alege un administrator");
        });
    }

    [Fact]
    public void Contact_Error_ShowsErrorAlert()
    {
        // Arrange
        _mockProfileService.Setup(x => x.GetAdminsAsync()).ThrowsAsync(new Exception("Network error"));
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.FindComponents<MudAlert>().Should().NotBeEmpty();
            cut.Markup.Should().Contain("Network error");
            cut.Markup.Should().Contain("Reincarca");
        });
    }

    [Fact]
    public void Contact_AdminCard_HasEmailLink()
    {
        // Arrange
        var admins = new List<ProfileDto>
        {
            new ProfileDto(Guid.NewGuid(), "admin@example.com", "Test Admin", 25, "F", "University", "Bio", "Style", "Interests", DateTime.UtcNow, null, "Admin")
        };
        _mockProfileService.Setup(x => x.GetAdminsAsync()).ReturnsAsync(admins);
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("mailto:admin@example.com");
        });
    }
    [Fact]
    public void Contact_AdminCard_DisplaysProfilePicture_WhenRelativePath()
    {
        // Arrange
        var admins = new List<ProfileDto>
        {
            new ProfileDto(Guid.NewGuid(), "admin@example.com", "Pic Admin", 25, "F", "Uni", "Bio", "Style", "Interests", DateTime.UtcNow, "/uploads/pic.jpg", "Admin")
        };
        _mockProfileService.Setup(x => x.GetAdminsAsync()).ReturnsAsync(admins);
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("src=\"http://localhost:5000/uploads/pic.jpg\"");
        });
    }

    [Fact]
    public void Contact_AdminCard_DisplaysProfilePicture_WhenAbsolutePath()
    {
        // Arrange
        var absoluteUrl = "https://example.com/pic.jpg";
        var admins = new List<ProfileDto>
        {
            new ProfileDto(Guid.NewGuid(), "admin@example.com", "Pic Admin", 25, "F", "Uni", "Bio", "Style", "Interests", DateTime.UtcNow, absoluteUrl, "Admin")
        };
        _mockProfileService.Setup(x => x.GetAdminsAsync()).ReturnsAsync(admins);
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain($"src=\"{absoluteUrl}\"");
        });
    }

    [Fact]
    public void Contact_Reload_RetriesLoadingAdmins()
    {
        // Arrange
        // First call throws
        _mockProfileService.SetupSequence(x => x.GetAdminsAsync())
            .ThrowsAsync(new Exception("Fail"))
            .ReturnsAsync(new List<ProfileDto>());
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert initial error state
        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Reincarca"));
        var reloadBtn = cut.FindComponents<MudButton>().First(b => b.Markup.Contains("Reincarca"));
        
        // Act - Click reload
        reloadBtn.Find("button").Click();

        // Assert - should reload and show empty message (or whatever second call returns)
        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Momentan nu exista administratori disponibili"));
        
        _mockProfileService.Verify(x => x.GetAdminsAsync(), Times.Exactly(2));
    }
}
