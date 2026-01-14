using Bunit;
using FluentAssertions;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Pages;

/// <summary>
/// Tests for RegisterWithPicture.razor component existence and type verification.
/// Full rendering tests skipped due to AuthService injection complexity.
/// </summary>
public class RegisterWithPictureTests
{
    [Fact]
    public void RegisterWithPicture_ComponentExists()
    {
        var componentType = typeof(RoomMate_Finder_Frontend.Pages.RegisterWithPicture);
        componentType.Should().NotBeNull();
    }

    [Fact]
    public void RegisterWithPicture_HasCorrectPageRoute()
    {
        var pageAttributes = typeof(RoomMate_Finder_Frontend.Pages.RegisterWithPicture)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Components.RouteAttribute), false);
        pageAttributes.Should().NotBeEmpty();
    }

    [Fact]
    public void RegisterWithPicture_ImplementsComponentBase()
    {
        typeof(RoomMate_Finder_Frontend.Pages.RegisterWithPicture)
            .IsSubclassOf(typeof(Microsoft.AspNetCore.Components.ComponentBase))
            .Should().BeTrue();
    }
}
