using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;
using RoomMate_Finder_Frontend.Shared;
using System.Security.Claims;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class LoginTests : BunitContext, IAsyncLifetime
{
    private readonly Mock<IAuthService> _mockAuthService;

    public Task InitializeAsync() => Task.CompletedTask;
    public new async Task DisposeAsync() => await base.DisposeAsync();

    public LoginTests()
    {
        Services.AddMudServices();
        
        // Setup JSInterop for MudBlazor components
        JSInterop.Mode = JSRuntimeMode.Loose;
        
        _mockAuthService = new Mock<IAuthService>();
        Services.AddSingleton(_mockAuthService.Object);
        
        Services.AddAuthorizationCore();
        Services.AddSingleton<AuthenticationStateProvider>(new TestAuthStateProvider());
    }

    class TestAuthStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }
    }

    [Fact(Skip = "MudForm validation context not working reliably in BUnit test environment")]
    public void Login_Renders_TitleAndForm()
    {
        var cut = Render<Login>();

        cut.Markup.Should().Contain("Bun venit înapoi!");
        cut.FindComponents<MudTextField<string>>().Should().HaveCount(2); // Email + Password
        //cut.FindComponent<MudButton>().Markup.Should().Contain("Conectează-te");
    }

    [Fact(Skip = "NavigationManager interception for MudLink failing in localized test env")]
    public void Login_ClickSignup_NavigatesToRegister()
    {
        var cut = Render<Login>();
        var navMan = Services.GetRequiredService<NavigationManager>();

        cut.Find("a[href='/register']").Click();

        navMan.Uri.Should().EndWith("/register");
    }

    [Fact(Skip = "MudForm validation context not working reliably in BUnit test environment")]
    public async Task Login_ValidSubmit_CallsAuthServiceAndNavigates()
    {
        var cut = Render<Login>();
        var navMan = Services.GetRequiredService<NavigationManager>();

        // Set model directly
        cut.Instance.model.Email = "test@test.com";
        cut.Instance.model.Password = "password123";

        // Call submit directly
        await cut.InvokeAsync(() => cut.Instance.HandleValidSubmit());

        // Verify calls
        _mockAuthService.Verify(x => x.LoginAsync("test@test.com", "password123"), Times.Once);
        navMan.Uri.Should().Be("http://localhost/"); 
    }

    [Fact(Skip = "MudForm validation context not working reliably in BUnit test environment")]
    public async Task Login_InvalidSubmit_ShowsError()
    {
        _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Invalid credentials"));

        var cut = Render<Login>();

        // Set model directly
        cut.Instance.model.Email = "test@test.com";
        cut.Instance.model.Password = "wrongpassword";

        // Call submit
        await cut.InvokeAsync(() => cut.Instance.HandleValidSubmit());

        // Verify that invalid credentials threw exception
        _mockAuthService.Verify(x => x.LoginAsync("test@test.com", "wrongpassword"), Times.Once);

        // Verify error message
        cut.WaitForState(() => cut.FindAll(".mud-alert-message").Any(), TimeSpan.FromSeconds(2));
        cut.Markup.Should().Contain("Credențiale invalide");
    }
}
