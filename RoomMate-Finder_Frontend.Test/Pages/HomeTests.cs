using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class HomeTests : BunitContext, IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    public HomeTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Home_Renders_HeroSection()
    {
        // Act
        var cut = Render<Home>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("RoomMate Finder");
            cut.Markup.Should().Contain("Găsește-ți colegul de cameră perfect");
        });
    }

    [Fact]
    public void Home_HasRegisterButton()
    {
        // Act
        var cut = Render<Home>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Începe acum");
            cut.Markup.Should().Contain("/register");
        });
    }

    [Fact]
    public void Home_HasLoginButton()
    {
        // Act
        var cut = Render<Home>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Login");
            cut.Markup.Should().Contain("/login");
        });
    }

    [Fact]
    public void Home_HasFeatureCards()
    {
        // Act
        var cut = Render<Home>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Matching Inteligent");
            cut.Markup.Should().Contain("Chat Securizat");
            cut.Markup.Should().Contain("Profiluri Verificate");
            cut.Markup.Should().Contain("Anunțuri de Camere");
            cut.FindComponents<MudCard>().Count.Should().BeGreaterThanOrEqualTo(4);
        });
    }

    [Fact]
    public void Home_HasHowItWorksSection()
    {
        // Act
        var cut = Render<Home>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Cum funcționează?");
            cut.Markup.Should().Contain("Creează-ți Profilul");
            cut.Markup.Should().Contain("Găsește Colegi");
            cut.Markup.Should().Contain("Conectează-te");
        });
    }

    [Fact]
    public void Home_HasCTASection()
    {
        // Act
        var cut = Render<Home>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Gata să-ți găsești colegul perfect?");
            cut.Markup.Should().Contain("Înregistrează-te Gratuit");
        });
    }

    [Fact]
    public void Home_UsesCorrectMudComponents()
    {
        // Act
        var cut = Render<Home>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.FindComponents<MudContainer>().Should().NotBeEmpty();
            cut.FindComponents<MudPaper>().Should().NotBeEmpty();
            cut.FindComponents<MudGrid>().Should().NotBeEmpty();
            cut.FindComponents<MudButton>().Should().NotBeEmpty();
        });
    }
}
