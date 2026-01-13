using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class ConversationsPageTests : BunitContext
{
    private readonly Mock<IConversationService> _mockConversationService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public ConversationsPageTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        _mockConversationService = new Mock<IConversationService>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        _mockConfiguration.Setup(c => c["ApiBaseUrl"]).Returns("http://localhost:5111");
        _mockNotificationService.Setup(n => n.HasUnreadMessages(It.IsAny<Guid>())).Returns(false);

        Services.AddSingleton(_mockConversationService.Object);
        Services.AddSingleton(_mockNotificationService.Object);
        Services.AddSingleton(_mockConfiguration.Object);
        
        Services.AddAuthorizationCore();
        Services.AddSingleton<AuthenticationStateProvider, TestConversationsAuthStateProvider>();
    }

    [Fact]
    public void Conversations_ShouldRenderTitle()
    {
        // Arrange
        _mockConversationService.Setup(x => x.GetConversationsAsync())
            .ReturnsAsync(new List<ConversationDto>());

        var authProvider = Services.GetService<AuthenticationStateProvider>() as TestConversationsAuthStateProvider;
        authProvider?.SetAuthenticatedUser(Guid.NewGuid(), "test@test.com", "User");

        // Act
        var cut = Render<Conversations>();

        // Assert
        cut.Markup.Should().Contain("Mesajele mele");
    }

    [Fact]
    public void Conversations_ShouldShowEmptyState_WhenNoConversations()
    {
        // Arrange
        _mockConversationService.Setup(x => x.GetConversationsAsync())
            .ReturnsAsync(new List<ConversationDto>());

        var authProvider = Services.GetService<AuthenticationStateProvider>() as TestConversationsAuthStateProvider;
        authProvider?.SetAuthenticatedUser(Guid.NewGuid(), "test@test.com", "User");

        // Act
        var cut = Render<Conversations>();
        cut.WaitForState(() => cut.Markup.Contains("Nicio conversație") || cut.Markup.Contains("încă"), TimeSpan.FromSeconds(3));

        // Assert
        cut.Markup.Should().Contain("Nicio conversație încă");
    }

    [Fact(Skip = "MudBlazor KeyInterceptorService async disposal issue")]
    public void Conversations_ShouldShowConversationList_WhenAvailable()
    {
        // Arrange
        var conversations = new List<ConversationDto>
        {
            new ConversationDto(Guid.NewGuid(), Guid.NewGuid(), "John Doe", "/images/john.jpg", "User", DateTime.UtcNow.AddHours(-1)),
            new ConversationDto(Guid.NewGuid(), Guid.NewGuid(), "Jane Smith", null, "Admin", DateTime.UtcNow.AddDays(-1))
        };

        _mockConversationService.Setup(x => x.GetConversationsAsync())
            .ReturnsAsync(conversations);

        var authProvider = Services.GetService<AuthenticationStateProvider>() as TestConversationsAuthStateProvider;
        authProvider?.SetAuthenticatedUser(Guid.NewGuid(), "test@test.com", "User");

        // Act
        var cut = Render<Conversations>();
        cut.WaitForState(() => cut.Markup.Contains("John Doe") || cut.Markup.Contains("Jane Smith"), TimeSpan.FromSeconds(3));

        // Assert
        cut.Markup.Should().Contain("John Doe");
        cut.Markup.Should().Contain("Jane Smith");
    }

    [Fact(Skip = "MudBlazor KeyInterceptorService async disposal issue")]
    public void Conversations_ShouldShowConversationCount()
    {
        // Arrange
        var conversations = new List<ConversationDto>
        {
            new ConversationDto(Guid.NewGuid(), Guid.NewGuid(), "User 1", null, "User", DateTime.UtcNow),
            new ConversationDto(Guid.NewGuid(), Guid.NewGuid(), "User 2", null, "User", DateTime.UtcNow),
            new ConversationDto(Guid.NewGuid(), Guid.NewGuid(), "User 3", null, "User", DateTime.UtcNow)
        };

        _mockConversationService.Setup(x => x.GetConversationsAsync())
            .ReturnsAsync(conversations);

        var authProvider = Services.GetService<AuthenticationStateProvider>() as TestConversationsAuthStateProvider;
        authProvider?.SetAuthenticatedUser(Guid.NewGuid(), "test@test.com", "User");

        // Act
        var cut = Render<Conversations>();
        cut.WaitForState(() => cut.Markup.Contains("3 conversații"), TimeSpan.FromSeconds(3));

        // Assert
        cut.Markup.Should().Contain("3 conversații");
    }

    public class TestConversationsAuthStateProvider : AuthenticationStateProvider
    {
        private AuthenticationState _state = new(new ClaimsPrincipal(new ClaimsIdentity()));

        public void SetAuthenticatedUser(Guid userId, string email, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.Name, email),
                new Claim("sub", userId.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Test");
            _state = new AuthenticationState(new ClaimsPrincipal(identity));
            NotifyAuthenticationStateChanged(Task.FromResult(_state));
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(_state);
        }
    }
}

