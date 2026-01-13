using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using System.Net;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class RegisterTests : IAsyncLifetime
{
    private readonly TestContext _ctx = new();
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

    public RegisterTests()
    {
        _ctx.Services.AddMudServices();
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        
        // Mock Http Client
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        _ctx.Services.AddSingleton(httpClient);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _ctx.DisposeAsync();
    }

    [Fact]
    public void Register_RendersCorrectly()
    {
        var cut = _ctx.Render<MudPopoverProvider>();
        var register = _ctx.Render<Register>();
        
        register.Find("h4").TextContent.Should().Contain("Alătură-te comunității");
    }

    [Fact]
    public void Register_ValidSubmit_FormFieldsArePresent()
    {
        // Arrange
        var cut = _ctx.Render<MudPopoverProvider>();
        var register = _ctx.Render<Register>();
        
        // Assert - verify form fields exist
        var textFields = register.FindComponents<MudTextField<string>>();
        textFields.Should().NotBeEmpty("Form should have text fields");
        
        // Check for expected labels
        register.Markup.Should().Contain("Nume complet");
        register.Markup.Should().Contain("Email");
        register.Markup.Should().Contain("Parola");
    }
}
