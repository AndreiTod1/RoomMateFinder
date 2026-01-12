using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using Xunit;

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
        cut.FindAll("a").Should().Contain(e => e.HasAttribute("href") && e.GetAttribute("href").Contains("register"));
    }
}
