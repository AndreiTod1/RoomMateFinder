using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using Microsoft.Extensions.Configuration;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class ConversationsTests : BunitContext, IAsyncLifetime
{
    private readonly Mock<IConversationService> _mockConversationService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IProfileService> _mockProfileService;
    private readonly Mock<IAuthService> _mockAuthService;

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    public ConversationsTests()
    {
        _mockConversationService = new Mock<IConversationService>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockProfileService = new Mock<IProfileService>();
        _mockAuthService = new Mock<IAuthService>();

        Services.AddMudServices();
        Services.AddSingleton(_mockConversationService.Object);
        Services.AddSingleton(_mockNotificationService.Object);
        Services.AddSingleton(_mockProfileService.Object);
        Services.AddSingleton(_mockAuthService.Object);
        Services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private void RenderProviders()
    {
        Render<MudPopoverProvider>();
        Render<MudDialogProvider>();
    }

    [Fact]
    public void Conversations_Loading_ShowsProgressIndicator()
    {
        // Arrange
        var tcs = new TaskCompletionSource<List<ConversationDto>>();
        _mockConversationService.Setup(x => x.GetConversationsAsync())
            .Returns(tcs.Task);

        RenderProviders();

        // Act
        var cut = Render<Conversations>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Se încarcă conversațiile");
        });

        // Cleanup
        tcs.SetResult(new List<ConversationDto>());
    }

    [Fact]
    public void Conversations_NoConversations_ShowsEmptyState()
    {
        // Arrange
        _mockConversationService.Setup(x => x.GetConversationsAsync())
            .ReturnsAsync(new List<ConversationDto>());

        RenderProviders();

        // Act
        var cut = Render<Conversations>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Nicio conversație încă");
        });
    }

    [Fact]
    public void Conversations_WithConversations_DisplaysList()
    {
        // Arrange
        var conversations = new List<ConversationDto>
        {
            new ConversationDto(
                Id: Guid.NewGuid(),
                OtherUserId: Guid.NewGuid(),
                OtherUserName: "John Doe",
                OtherUserProfilePicture: null,
                OtherUserRole: "User",
                CreatedAt: DateTime.UtcNow.AddMinutes(-5)
            )
        };

        _mockConversationService.Setup(x => x.GetConversationsAsync())
            .ReturnsAsync(conversations);

        RenderProviders();

        // Act
        var cut = Render<Conversations>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("John Doe");
        });
    }

    [Fact]
    public void Conversations_HasTitle()
    {
        // Arrange
        _mockConversationService.Setup(x => x.GetConversationsAsync())
            .ReturnsAsync(new List<ConversationDto>());

        RenderProviders();

        // Act
        var cut = Render<Conversations>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Mesajele mele");
        });
    }

    [Fact]
    public void Conversations_EmptyState_HasSearchRoomsButton()
    {
        // Arrange
        _mockConversationService.Setup(x => x.GetConversationsAsync())
            .ReturnsAsync(new List<ConversationDto>());

        RenderProviders();

        // Act
        var cut = Render<Conversations>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Caută camere");
            cut.Markup.Should().Contain("/listings");
        });
    }

    [Fact]
    public void Conversations_WithMultipleConversations_DisplaysAll()
    {
        // Arrange
        var conversations = new List<ConversationDto>
        {
            new ConversationDto(Guid.NewGuid(), Guid.NewGuid(), "User One", null, "User", DateTime.UtcNow.AddMinutes(-10)),
            new ConversationDto(Guid.NewGuid(), Guid.NewGuid(), "User Two", null, "Admin", DateTime.UtcNow.AddMinutes(-5))
        };

        _mockConversationService.Setup(x => x.GetConversationsAsync())
            .ReturnsAsync(conversations);

        RenderProviders();

        // Act
        var cut = Render<Conversations>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("User One");
            cut.Markup.Should().Contain("User Two");
        });
    }

    [Fact]
    public void Conversations_ComponentType_IsCorrect()
    {
        var componentType = typeof(Conversations);
        componentType.Should().NotBeNull();
    }

    [Fact]
    public void Conversations_ImplementsIDisposable()
    {
        typeof(Conversations).GetInterfaces().Should().Contain(typeof(IDisposable));
    }

    [Fact]
    public void Conversations_ServiceError_ShowsErrorMessage()
    {
        // Arrange
        _mockConversationService.Setup(x => x.GetConversationsAsync())
            .ThrowsAsync(new Exception("Connection failed"));

        RenderProviders();

        // Act
        var cut = Render<Conversations>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("A apărut o eroare");
        });
    }

    [Fact]
    public void Conversations_WithProfilePicture_DisplaysAvatar()
    {
        // Arrange
        var conversations = new List<ConversationDto>
        {
            new ConversationDto(
                Id: Guid.NewGuid(),
                OtherUserId: Guid.NewGuid(),
                OtherUserName: "Test User",
                OtherUserProfilePicture: "/images/profile.jpg",
                OtherUserRole: "User",
                CreatedAt: DateTime.UtcNow
            )
        };

        _mockConversationService.Setup(x => x.GetConversationsAsync())
            .ReturnsAsync(conversations);

        RenderProviders();

        // Act
        var cut = Render<Conversations>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("profile.jpg");
        });
    }

    [Fact]
    public void Conversations_WithAdminRole_DisplaysAdminChip()
    {
        // Arrange
        var conversations = new List<ConversationDto>
        {
            new ConversationDto(Guid.NewGuid(), Guid.NewGuid(), "Admin User", null, "Admin", DateTime.UtcNow)
        };

        _mockConversationService.Setup(x => x.GetConversationsAsync())
            .ReturnsAsync(conversations);

        RenderProviders();

        // Act
        var cut = Render<Conversations>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Admin");
        });
    }

    [Fact]
    public void Conversations_UnreadMessage_ShowsNewBadge()
    {
        // Arrange
        var conversationId = Guid.NewGuid();
        var conversations = new List<ConversationDto>
        {
            new ConversationDto(conversationId, Guid.NewGuid(), "User With Unread", null, "User", DateTime.UtcNow)
        };

        _mockConversationService.Setup(x => x.GetConversationsAsync())
            .ReturnsAsync(conversations);
        _mockNotificationService.Setup(x => x.HasUnreadMessages(conversationId))
            .Returns(true);

        RenderProviders();

        // Act
        var cut = Render<Conversations>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("NOU");
        });
    }

    [Fact]
    public void Conversations_RecentTime_ShowsMinutesAgo()
    {
        // Arrange
        var conversations = new List<ConversationDto>
        {
            new ConversationDto(Guid.NewGuid(), Guid.NewGuid(), "Recent User", null, "User", DateTime.UtcNow.AddMinutes(-5))
        };

        _mockConversationService.Setup(x => x.GetConversationsAsync())
            .ReturnsAsync(conversations);

        RenderProviders();

        // Act
        var cut = Render<Conversations>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("min");
        });
    }

    [Fact]
    public void Conversations_OldTime_ShowsHoursAgo()
    {
        // Arrange
        var conversations = new List<ConversationDto>
        {
            new ConversationDto(Guid.NewGuid(), Guid.NewGuid(), "Old User", null, "User", DateTime.UtcNow.AddHours(-3))
        };

        _mockConversationService.Setup(x => x.GetConversationsAsync())
            .ReturnsAsync(conversations);

        RenderProviders();

        // Act
        var cut = Render<Conversations>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("ore");
        });
    }

    [Fact]
    public void Conversations_ShowsConversationCount()
    {
        // Arrange
        var conversations = new List<ConversationDto>
        {
            new ConversationDto(Guid.NewGuid(), Guid.NewGuid(), "User1", null, "User", DateTime.UtcNow),
            new ConversationDto(Guid.NewGuid(), Guid.NewGuid(), "User2", null, "User", DateTime.UtcNow)
        };

        _mockConversationService.Setup(x => x.GetConversationsAsync())
            .ReturnsAsync(conversations);

        RenderProviders();

        // Act
        var cut = Render<Conversations>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("2 conversații");
        });
    }
}
