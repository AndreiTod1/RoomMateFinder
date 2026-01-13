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

public class ConversationsTests : TestContext, IAsyncLifetime
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
        Render<MudSnackbarProvider>();
    }

    [Fact]
    public async Task Conversations_Loading_ShowsProgressIndicator()
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
}
