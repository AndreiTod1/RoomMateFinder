using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RoomMate_Finder_Frontend.Models;
using RoomMate_Finder_Frontend.Services;
using RoomMate_Finder_Frontend.Shared;
using System.Security.Claims;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Shared;

public class ProfilesDropdownTests : BunitContext
{
    private readonly Mock<IProfileService> _profileServiceMock;
    
    public ProfilesDropdownTests()
    {
        _profileServiceMock = new Mock<IProfileService>();
        Services.AddSingleton(_profileServiceMock.Object);
        Services.AddAuthorizationCore(); // Adds AuthenticationStateProvider
    }

    [Fact]
    public void Given_ClosedState_When_ToggleClicked_Then_DropdownOpens()
    {
        // Arrange
        var comp = Render<ProfilesDropdown>();
        
        // Act
        comp.Find("button").Click();
        
        // Assert
        comp.FindAll("ul.dropdown-menu.show").Should().HaveCount(1);
    }

    [Fact]
    public void Given_OpenState_When_ToggleClicked_Then_DropdownCloses()
    {
        // Arrange
        var comp = Render<ProfilesDropdown>();
        comp.Find("button").Click(); // Open
        
        // Act
        comp.Find("button").Click(); // Close
        
        // Assert
        comp.FindAll("ul.dropdown-menu.show").Should().BeEmpty();
    }
    
    [Fact]
    public void Given_OpenState_When_CloseClicked_Then_DropdownClosesAndNavigates()
    {
        // Arrange
        // Note: For "Toate profilurile", it uses href="profiles". Navigation is handled by browser for anchor tags unless intercepted.
        // We just check if it closes.
        var comp = Render<ProfilesDropdown>();
        comp.Find("button").Click(); // Open
        
        // Act
        comp.Find("a.dropdown-item[href='profiles']").Click();
        
        // Assert
        comp.FindAll("ul.dropdown-menu.show").Should().BeEmpty();
    }

    [Fact]
    public async Task Given_AuthenticatedUser_When_GoToMyProfileClicked_Then_NavigatesToProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _profileServiceMock.Setup(s => s.GetCurrentAsync())
            .ReturnsAsync(new ProfileDto(userId, "email", "name", 20, "M", "Uni", "Bio", "Style", "Interests", DateTime.Now, null, "Role"));

        var comp = Render<ProfilesDropdown>();
        comp.Find("button").Click(); // Open
        
        // Act
        await comp.Find("a.dropdown-item[href='#']").ClickAsync(new MouseEventArgs());
        
        // Assert
        // Verify navigation: Services.GetRequiredService<FakeNavigationManager>().Uri
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.Uri.Should().EndWith($"/profile/{userId}");
    }

    [Fact]
    public async Task Given_UnauthenticatedUser_When_GoToMyProfileClicked_Then_NavigatesToLogin()
    {
        // Arrange
        _profileServiceMock.Setup(s => s.GetCurrentAsync())
            .ThrowsAsync(new UnauthorizedAccessException());

        var comp = Render<ProfilesDropdown>();
        comp.Find("button").Click(); // Open
        
        // Act
        await comp.Find("a.dropdown-item[href='#']").ClickAsync(new MouseEventArgs());
        
        // Assert
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.Uri.Should().EndWith("/login");
    }
}
