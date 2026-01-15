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
using RoomMate_Finder_Frontend.Models;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;
using System.Security.Claims;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class MyMatchesTests : BunitContext, IAsyncLifetime
{
    private readonly Mock<IMatchingService> _mockMatchingService;
    private readonly Mock<IProfileService> _mockProfileService;
    private readonly Mock<IConversationService> _mockConversationService;
    private readonly Mock<ISnackbar> _mockSnackbar;
    private readonly AuthenticationState _authState;
    private readonly Guid _userId = Guid.NewGuid();

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    public MyMatchesTests()
    {
        _mockMatchingService = new Mock<IMatchingService>();
        _mockProfileService = new Mock<IProfileService>();
        _mockConversationService = new Mock<IConversationService>();
        _mockSnackbar = new Mock<ISnackbar>();

        Services.AddMudServices();
        Services.AddSingleton(_mockMatchingService.Object);
        Services.AddSingleton(_mockProfileService.Object);
        Services.AddSingleton(_mockConversationService.Object);
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
            new Claim(ClaimTypes.NameIdentifier, _userId.ToString()),
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

    private IRenderedComponent<MyMatches> RenderComponent()
    {
        return Render<MyMatches>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));
    }
    
    private void RenderProviders()
    {
        Render<MudPopoverProvider>();
        Render<MudDialogProvider>();
    }

    [Fact]
    public void MyMatches_Loading_ShowsProgressCircular()
    {
        // Arrange
        var tcs = new TaskCompletionSource<ProfileDto?>();
        _mockProfileService.Setup(x => x.GetCurrentAsync()).Returns(tcs.Task);
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.FindComponents<MudProgressCircular>().Should().NotBeEmpty();

        // Cleanup
        tcs.SetResult(null);
    }

    [Fact]
    public void MyMatches_NoMatches_ShowsEmptyState()
    {
        // Arrange
        var profile = new ProfileDto(_userId, "test@test.com", "Test User", 20, "M", "Uni", "Bio", "Style", "Interests", DateTime.UtcNow, null);
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(profile);
        _mockMatchingService.Setup(x => x.GetMyMatchesAsync(_userId)).ReturnsAsync(new List<UserMatchDto>());
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Încă nu ai niciun match");
            cut.Markup.Should().Contain("Descoperă Profiluri");
        });
    }

    [Fact]
    public void MyMatches_WithMatches_DisplaysMatchCards()
    {
        // Arrange
        var profile = new ProfileDto(_userId, "test@test.com", "Test User", 20, "M", "Uni", "Bio", "Style", "Interests", DateTime.UtcNow, null);
        var matches = new List<UserMatchDto>
        {
            new UserMatchDto(Guid.NewGuid(), Guid.NewGuid(), "match@test.com", "Match User", 22, "F", "Uni B", "Bio text", "Active", "Gaming, Music", DateTime.UtcNow, true, null)
        };
        
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(profile);
        _mockMatchingService.Setup(x => x.GetMyMatchesAsync(_userId)).ReturnsAsync(matches);
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Match User");
            cut.Markup.Should().Contain("22 ani");
            cut.Markup.Should().Contain("Uni B");
            cut.FindComponents<MudCard>().Should().NotBeEmpty();
        });
    }

    [Fact]
    public void MyMatches_HasTitle()
    {
        // Arrange
        var profile = new ProfileDto(_userId, "test@test.com", "Test User", 20, "M", "Uni", "Bio", "Style", "Interests", DateTime.UtcNow, null);
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(profile);
        _mockMatchingService.Setup(x => x.GetMyMatchesAsync(_userId)).ReturnsAsync(new List<UserMatchDto>());
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Match-urile Mele");
        });
    }

    [Fact]
    public void MyMatches_MatchCard_HasMessageButton()
    {
        // Arrange
        var profile = new ProfileDto(_userId, "test@test.com", "Test User", 20, "M", "Uni", "Bio", "Style", "Interests", DateTime.UtcNow, null);
        var matches = new List<UserMatchDto>
        {
            new UserMatchDto(Guid.NewGuid(), Guid.NewGuid(), "match@test.com", "Match User", 22, "F", "Uni", "Bio", "Active", "Gaming", DateTime.UtcNow, true, null)
        };
        
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(profile);
        _mockMatchingService.Setup(x => x.GetMyMatchesAsync(_userId)).ReturnsAsync(matches);
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Mesaj");
        });
    }

    [Fact]
    public void MyMatches_Error_ShowsAlert()
    {
        // Arrange
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ThrowsAsync(new Exception("Test error"));
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.FindComponents<MudAlert>().Should().NotBeEmpty();
            cut.Markup.Should().Contain("Test error");
        });
    }

    [Fact]
    public async Task MyMatches_ClickMessage_StartsNewConversation()
    {
        // Arrange
        var profile = new ProfileDto(_userId, "test@test.com", "Test User", 20, "M", "Uni", "Bio", "Style", "Interests", DateTime.UtcNow, null);
        var matchUserId = Guid.NewGuid();
        var matches = new List<UserMatchDto>
        {
            new UserMatchDto(Guid.NewGuid(), matchUserId, "match@test.com", "Match User", 22, "F", "Uni", "Bio", "Active", "Gaming", DateTime.UtcNow, true, null)
        };
        var newConversation = new ConversationDto(Guid.NewGuid(), matchUserId, "Match User", null, "User", DateTime.UtcNow);
        
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(profile);
        _mockMatchingService.Setup(x => x.GetMyMatchesAsync(_userId)).ReturnsAsync(matches);
        _mockConversationService.Setup(x => x.GetConversationsAsync()).ReturnsAsync(new List<ConversationDto>());
        _mockConversationService.Setup(x => x.StartConversationAsync(matchUserId)).ReturnsAsync(newConversation);
        _mockSnackbar.Setup(x => x.Configuration).Returns(new SnackbarConfiguration());
        
        RenderProviders();
        var cut = RenderComponent();

        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Mesaj"));

        // Act
        var messageBtn = cut.FindComponents<MudButton>().First(b => b.Markup.Contains("Mesaj"));
        await cut.InvokeAsync(() => messageBtn.Instance.OnClick.InvokeAsync(null));

        // Assert
        _mockConversationService.Verify(x => x.StartConversationAsync(matchUserId), Times.Once);
    }

    [Fact]
    public async Task MyMatches_ClickMessage_NavigatesToExistingConversation()
    {
        // Arrange
        var profile = new ProfileDto(_userId, "test@test.com", "Test User", 20, "M", "Uni", "Bio", "Style", "Interests", DateTime.UtcNow, null);
        var matchUserId = Guid.NewGuid();
        var existingConvId = Guid.NewGuid();
        var matches = new List<UserMatchDto>
        {
            new UserMatchDto(Guid.NewGuid(), matchUserId, "match@test.com", "Match User", 22, "F", "Uni", "Bio", "Active", "Gaming", DateTime.UtcNow, true, null)
        };
        var existingConversation = new ConversationDto(existingConvId, matchUserId, "Match User", null, "User", DateTime.UtcNow);
        
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(profile);
        _mockMatchingService.Setup(x => x.GetMyMatchesAsync(_userId)).ReturnsAsync(matches);
        _mockConversationService.Setup(x => x.GetConversationsAsync()).ReturnsAsync(new List<ConversationDto> { existingConversation });
        
        RenderProviders();
        var navMan = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent();

        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Mesaj"));

        // Act
        var messageBtn = cut.FindComponents<MudButton>().First(b => b.Markup.Contains("Mesaj"));
        await cut.InvokeAsync(() => messageBtn.Instance.OnClick.InvokeAsync(null));

        // Assert - should NOT start new conversation
        _mockConversationService.Verify(x => x.StartConversationAsync(It.IsAny<Guid>()), Times.Never);
        navMan.Uri.Should().Contain($"/conversations/{existingConvId}");
    }

    [Fact]
    public void MyMatches_NullProfile_RedirectsToLogin()
    {
        // Arrange
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync((ProfileDto?)null);
        
        RenderProviders();
        var navMan = Services.GetRequiredService<NavigationManager>();
        
        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            navMan.Uri.Should().EndWith("/login");
        });
    }

    [Fact]
    public void MyMatches_WithProfilePicture_DisplaysImage()
    {
        // Arrange
        var profile = new ProfileDto(_userId, "test@test.com", "Test User", 20, "M", "Uni", "Bio", "Style", "Interests", DateTime.UtcNow, null);
        var matches = new List<UserMatchDto>
        {
            new UserMatchDto(Guid.NewGuid(), Guid.NewGuid(), "match@test.com", "Match User", 22, "F", "Uni", "Bio", "Active", "Gaming", DateTime.UtcNow, true, "/uploads/profile.jpg")
        };
        
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(profile);
        _mockMatchingService.Setup(x => x.GetMyMatchesAsync(_userId)).ReturnsAsync(matches);
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("img");
            cut.Markup.Should().Contain("/uploads/profile.jpg");
        });
    }

    [Fact]
    public void MyMatches_WithoutProfilePicture_DisplaysInitials()
    {
        // Arrange
        var profile = new ProfileDto(_userId, "test@test.com", "Test User", 20, "M", "Uni", "Bio", "Style", "Interests", DateTime.UtcNow, null);
        var matches = new List<UserMatchDto>
        {
            new UserMatchDto(Guid.NewGuid(), Guid.NewGuid(), "match@test.com", "John Doe", 22, "F", "Uni", "Bio", "Active", "Gaming", DateTime.UtcNow, true, null)
        };
        
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(profile);
        _mockMatchingService.Setup(x => x.GetMyMatchesAsync(_userId)).ReturnsAsync(matches);
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("JD"); // Initials for John Doe
        });
    }

    [Fact]
    public void MyMatches_WithMatchInterests_DisplaysChips()
    {
        // Arrange
        var profile = new ProfileDto(_userId, "test@test.com", "Test User", 20, "M", "Uni", "Bio", "Style", "Interests", DateTime.UtcNow, null);
        var matches = new List<UserMatchDto>
        {
            new UserMatchDto(Guid.NewGuid(), Guid.NewGuid(), "match@test.com", "Match User", 22, "F", "Uni", "Bio", "Active", "Gaming, Music, Sports", DateTime.UtcNow, true, null)
        };
        
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(profile);
        _mockMatchingService.Setup(x => x.GetMyMatchesAsync(_userId)).ReturnsAsync(matches);
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Gaming");
            cut.Markup.Should().Contain("Music");
            cut.Markup.Should().Contain("+1"); // 3 interests, show 2, +1 remaining
        });
    }

    [Fact]
    public void MyMatches_WithBio_DisplaysBio()
    {
        // Arrange
        var profile = new ProfileDto(_userId, "test@test.com", "Test User", 20, "M", "Uni", "Bio", "Style", "Interests", DateTime.UtcNow, null);
        var matches = new List<UserMatchDto>
        {
            new UserMatchDto(Guid.NewGuid(), Guid.NewGuid(), "match@test.com", "Match User", 22, "F", "Uni", "I love coding!", "Active", "Gaming", DateTime.UtcNow, true, null)
        };
        
        _mockProfileService.Setup(x => x.GetCurrentAsync()).ReturnsAsync(profile);
        _mockMatchingService.Setup(x => x.GetMyMatchesAsync(_userId)).ReturnsAsync(matches);
        
        RenderProviders();

        // Act
        var cut = RenderComponent();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("I love coding!");
        });
    }
}

