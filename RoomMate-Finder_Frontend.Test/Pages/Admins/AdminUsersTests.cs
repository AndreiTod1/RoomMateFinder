using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Web;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages.Admins;
using RoomMate_Finder_Frontend.Services;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Pages.Admins;

public class AdminUsersTests : TestContext, IAsyncLifetime
{
    private readonly Mock<IProfileService> _mockProfileService;
    private readonly Mock<IDialogService> _mockDialogService;

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    public AdminUsersTests()
    {
        _mockProfileService = new Mock<IProfileService>();
        _mockDialogService = new Mock<IDialogService>();

        Services.AddMudServices();
        Services.AddSingleton(_mockProfileService.Object);
        Services.AddSingleton(_mockDialogService.Object);
        
        // Add HttpClient for image URL generation
        Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri("https://localhost:5001/") });
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private void RenderProviders()
    {
        Render<MudPopoverProvider>();
        Render<MudDialogProvider>();
        Render<MudSnackbarProvider>();
    }

    [Fact]
    public async Task AdminUsers_Loading_ShowsProgressIndicator()
    {
        // Arrange
        var tcs = new TaskCompletionSource<PaginatedUsersResponse>();
        _mockProfileService.Setup(x => x.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .Returns(tcs.Task);

        RenderProviders();

        // Act
        var cut = Render<AdminUsers>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("mud-progress-linear");
        });

        // Cleanup
        tcs.SetResult(new PaginatedUsersResponse(new List<UserDto>(), 0, 1, 15));
    }

    [Fact]
    public void AdminUsers_NoUsers_ShowsEmptyState()
    {
        // Arrange
        _mockProfileService.Setup(x => x.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new PaginatedUsersResponse(new List<UserDto>(), 0, 1, 15));

        RenderProviders();

        // Act
        var cut = Render<AdminUsers>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("No users found");
        });
    }

    [Fact]
    public void AdminUsers_WithUsers_DisplaysUserList()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new UserDto(
                Guid.NewGuid(), 
                "john@example.com", 
                "John Doe", 
                25, 
                "Male", 
                "University", 
                null, 
                DateTime.UtcNow, 
                "User")
        };

        _mockProfileService.Setup(x => x.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new PaginatedUsersResponse(users, 1, 1, 15));

        RenderProviders();

        // Act
        var cut = Render<AdminUsers>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("John Doe");
            cut.Markup.Should().Contain("john@example.com");
        });
    }

    [Fact]
    public async Task AdminUsers_DeleteUser_CallsServiceAndReloads()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var users = new List<UserDto>
        {
            new UserDto(
                userId, 
                "delete@example.com", 
                "Delete Me", 
                25, 
                "Male", 
                "Uni", 
                null, 
                DateTime.UtcNow, 
                "User")
        };

        _mockProfileService.Setup(x => x.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new PaginatedUsersResponse(users, 1, 1, 15));

        _mockDialogService.Setup(x => x.ShowMessageBox(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DialogOptions>()))
            .ReturnsAsync(true);

        _mockProfileService.Setup(x => x.DeleteProfileAsync(userId))
            .Returns(Task.CompletedTask);

        RenderProviders();

        var cut = Render<AdminUsers>();
        cut.WaitForState(() => cut.Markup.Contains("Delete Me"));

        // Act
        // Find the delete button which has Color.Error
        var deleteButton = cut.FindComponents<MudIconButton>()
            .FirstOrDefault(c => c.Instance.Color == Color.Error);
        
        if (deleteButton != null)
        {
            deleteButton.Find("button").Click();
        }

        // Assert
        _mockProfileService.Verify(x => x.DeleteProfileAsync(userId), Times.Once);
    }
}
