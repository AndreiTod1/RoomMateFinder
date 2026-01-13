using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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

    [Fact(Skip = "Requires complex JS mocking for MudForm validation")]
    public void Login_ClickingSubmit_CallsAuthService()
    {
        // Arrange
        // Setup successful login mock
        _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var cut = Render<Login>();
        
        // Simpler way with bUnit: FindComponent<MudTextField<string>>
        var textFields = cut.FindComponents<MudBlazor.MudTextField<string>>();
        var emailComponent = textFields[0]; 
        var passwordComponent = textFields[1];

        // Act - Simulate typing
        #pragma warning disable BL0005 // Component parameter should not be set outside of its component
        emailComponent.Instance.Value = "test@test.com";
        passwordComponent.Instance.Value = "password123";
        #pragma warning restore BL0005

        // Find submit button
        var button = cut.Find("button");
        button.Click();

        // Assert
        // Allow time for async submit
        cut.WaitForState(() => _mockAuthService.Invocations.Count > 0, TimeSpan.FromSeconds(2));
        
        _mockAuthService.Verify(x => x.LoginAsync("test@test.com", "password123"), Times.Once);
    }
}
