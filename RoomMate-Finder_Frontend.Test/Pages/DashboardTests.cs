using Bunit;
using FluentAssertions;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class DashboardTests : BunitContext
{
    public DashboardTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Home_RendersCorrectly()
    {
        // Act
        var cut = Render<Home>();

        // Assert
        cut.Markup.Should().Contain("Găsește-ți colegul");
        var links = cut.FindAll("a");
        var hasRegisterLink = links.Any(e => 
        {
            var href = e.GetAttribute("href");
            return href != null && href.Contains("register");
        });
        hasRegisterLink.Should().BeTrue();
    }
}
