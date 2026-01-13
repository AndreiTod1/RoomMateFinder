using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class LoginTests : BunitContext
{
    private readonly Mock<IAuthService> _mockAuthService;

    public LoginTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        _mockAuthService = new Mock<IAuthService>();
        Services.AddSingleton(_mockAuthService.Object);
        // NavigationManager is added by default by bUnit
    }

    [Fact]
    public void Login_RendersCorrectly()
    {
        // Act
        var cut = Render<Login>();

        // Assert
        cut.Find("h4").TextContent.Should().Contain("Bun venit"); // "Bun venit înapoi!"
        cut.FindAll("input").Count.Should().BeGreaterThan(0); // Should have email and password fields
    }

    [Fact]
    public async Task Login_ClickingSubmit_CallsAuthService()
    {
        // Arrange
        // Setup successful login mock
        _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var cut = Render<Login>();
        
        // Act - Simulate typing via DOM events to trigger binding
        var inputs = cut.FindAll("input");
        inputs[0].Change("test@test.com");
        inputs[1].Change("password123");

        // Find submit button via Component
        var btn = cut.FindComponent<MudButton>();
        // Must invoke on dispatcher
        await cut.InvokeAsync(() => btn.Instance.OnClick.InvokeAsync(new MouseEventArgs()));

        // Assert
        // Allow time for validation and async submit
        // Validation is async in MudBlazor, so we need to wait
        // Also checking for success
        cut.WaitForState(() => _mockAuthService.Invocations.Count > 0, TimeSpan.FromSeconds(2));
        
        _mockAuthService.Verify(x => x.LoginAsync("test@test.com", "password123"), Times.Once);
    }
}
