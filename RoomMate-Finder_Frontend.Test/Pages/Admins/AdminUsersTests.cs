using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages.Admins;
using RoomMate_Finder_Frontend.Services;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Pages.Admins;

public class AdminUsersTests : BunitContext, IAsyncLifetime
{
    private readonly Mock<IProfileService> _mockProfileService;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly Mock<ISnackbar> _mockSnackbar;

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    public AdminUsersTests()
    {
        _mockProfileService = new Mock<IProfileService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockSnackbar = new Mock<ISnackbar>();
        _mockSnackbar.Setup(x => x.Configuration).Returns(new SnackbarConfiguration());

        Services.AddMudServices();
        Services.AddSingleton(_mockProfileService.Object);
        Services.AddSingleton(_mockDialogService.Object);
        Services.AddSingleton(_mockSnackbar.Object); // Overwrites AddMudServices?
        
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
    public void AdminUsers_Loading_ShowsProgressIndicator()
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
    public void AdminUsers_DeleteUser_CallsServiceAndReloads()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var users = new List<UserDto>
        {
            new UserDto(userId, "delete@example.com", "Delete Me", 25, "Male", "Uni", null, DateTime.UtcNow, "User")
        };

        _mockProfileService.Setup(x => x.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new PaginatedUsersResponse(users, 1, 1, 15));

        _mockDialogService.Setup(x => x.ShowMessageBox(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DialogOptions>()))
            .ReturnsAsync(true);

        _mockProfileService.Setup(x => x.DeleteProfileAsync(userId)).Returns(Task.CompletedTask);

        RenderProviders();
        var cut = Render<AdminUsers>();
        cut.WaitForState(() => cut.Markup.Contains("Delete Me"));

        // Act
        // Find the delete button which has Color.Error
        var deleteButton = cut.FindComponents<MudIconButton>().First(c => c.Instance.Icon == Icons.Material.Filled.DeleteOutline);
        deleteButton.Find("button").Click();

        // Assert
        _mockProfileService.Verify(x => x.DeleteProfileAsync(userId), Times.Once);
        // Should reload
        _mockProfileService.Verify(x => x.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task AdminUsers_ToggleRole_PromoteUser_CallsService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var users = new List<UserDto>
        {
            new UserDto(userId, "user@test.com", "User Name", 25, "M", "Uni", null, DateTime.UtcNow, "User")
        };

        _mockProfileService.Setup(x => x.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new PaginatedUsersResponse(users, 1, 1, 15));

        _mockDialogService.Setup(x => x.ShowMessageBox(
            It.IsAny<string>(), It.IsAny<string>(),It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DialogOptions>()))
            .ReturnsAsync(true);

        RenderProviders();
        var cut = Render<AdminUsers>();
        cut.WaitForState(() => cut.Markup.Contains("User Name"));

        // Act
        // Find Promote button (Up Arrow)
        var btn = cut.FindComponents<MudIconButton>()
            .First(b => b.Instance.Icon == Icons.Material.Filled.ArrowUpward);
        
        await cut.InvokeAsync(() => btn.Instance.OnClick.InvokeAsync(null));

        // Assert
        _mockProfileService.Verify(x => x.UpdateRoleAsync(userId, "Admin"), Times.Once);
    }
    
    [Fact]
    public async Task AdminUsers_ToggleRole_DemoteAdmin_CallsService()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var users = new List<UserDto>
        {
            new UserDto(userId, "admin@test.com", "Admin Name", 25, "M", "Uni", null, DateTime.UtcNow, "Admin")
        };

        _mockProfileService.Setup(x => x.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new PaginatedUsersResponse(users, 1, 1, 15));

        _mockDialogService.Setup(x => x.ShowMessageBox(
             It.IsAny<string>(), It.IsAny<string>(),It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DialogOptions>()))
            .ReturnsAsync(true);

        RenderProviders();
        var cut = Render<AdminUsers>();
        cut.WaitForState(() => cut.Markup.Contains("Admin Name"));

        // Act
        // Find Demote button (Down Arrow)
        var btn = cut.FindComponents<MudIconButton>()
            .First(b => b.Instance.Icon == Icons.Material.Filled.ArrowDownward);
        
        await cut.InvokeAsync(() => btn.Instance.OnClick.InvokeAsync(null));

        // Assert
        _mockProfileService.Verify(x => x.UpdateRoleAsync(userId, "User"), Times.Once);
    }

    [Fact]
    public void AdminUsers_Search_OnEnter_ReloadsUsers()
    {
        // Arrange
        _mockProfileService.Setup(x => x.GetAllUsersAsync(1, 15, It.IsAny<string>()))
            .ReturnsAsync(new PaginatedUsersResponse(new List<UserDto>(), 0, 1, 15));

        RenderProviders();
        var cut = Render<AdminUsers>();

        // Act
        var input = cut.Find("input");
        input.Input("SearchQuery");
        input.KeyDown(new KeyboardEventArgs { Key = "Enter" });

        // Assert
        _mockProfileService.Verify(x => x.GetAllUsersAsync(1, 15, "SearchQuery"), Times.Once);
    }

    [Fact]
    public void AdminUsers_NavigateToProfile_Redirects()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var users = new List<UserDto>
        {
             new UserDto(userId, "unique@u.com", "AppUser", 20, "M", "Uni", null, DateTime.UtcNow, "User")
        };
        _mockProfileService.Setup(x => x.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new PaginatedUsersResponse(users, 1, 1, 15));

        RenderProviders();
        var nav = Services.GetRequiredService<NavigationManager>();
        var cut = Render<AdminUsers>();
        cut.WaitForState(() => cut.Markup.Contains("unique@u.com"));

        // Act
        // Find div with semantic selector or verify structure. 
        // The div has @onclick.
        // We find the Avatar which is inside the div, then get parent? No BUnit doesn't support Parent navigation easily.
        // We search for div that contains the email.
        var row = cut.FindAll("div.d-flex.align-center.pa-4")
            .First(e => e.InnerHtml.Contains("unique@u.com"));
        
        row.Click();

        // Assert
        nav.Uri.Should().EndWith($"/profile/{userId}");
    }

    [Fact]
    public async Task AdminUsers_Pagination_ChangesPage()
    {
        // Arrange
        var users = new List<UserDto> { new UserDto(Guid.NewGuid(), "a@a.com", "A", 20, "M", "U", null, DateTime.UtcNow, "User") };
        _mockProfileService.Setup(x => x.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new PaginatedUsersResponse(users, 100, 7, 15)); 
            // 100 users = 7 pages

        RenderProviders();
        var cut = Render<AdminUsers>();
        cut.WaitForState(() => cut.Markup.Contains("mud-pagination"));

        // Act
        var pagination = cut.FindComponent<MudPagination>();
        await cut.InvokeAsync(() => pagination.Instance.SelectedChanged.InvokeAsync(2));

        // Assert
        _mockProfileService.Verify(x => x.GetAllUsersAsync(2, 15, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void AdminUsers_ServiceError_ShowsSnackbar()
    {
        // Arrange
        _mockProfileService.Setup(x => x.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Load Failed"));

        // Mock snackbar config again just in case
         
        RenderProviders();
        
        // Act
        var cut = Render<AdminUsers>();
        
        // Assert
        _mockSnackbar.Verify(x => x.Add(It.Is<string>(s => s.Contains("Load Failed")), Severity.Error, null, null), Times.Once);
    }
}
