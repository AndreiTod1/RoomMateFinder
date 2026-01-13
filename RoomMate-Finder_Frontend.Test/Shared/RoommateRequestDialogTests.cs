using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Shared;

namespace RoomMate_Finder_Frontend.Test.Shared;

public class RoommateRequestDialogTests : BunitContext, IAsyncLifetime
{
    public Task InitializeAsync()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        return Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    [Fact]
    public void RoommateRequestDialog_HasCorrectParameters()
    {
        // Assert that the component has expected parameter properties
        var componentType = typeof(RoommateRequestDialog);
        
        componentType.GetProperty("OtherUserName").Should().NotBeNull();
        componentType.GetProperty("IsConfirmation").Should().NotBeNull();
    }

    [Fact]
    public void RoommateRequestDialog_DefaultValues_AreCorrect()
    {
        // Create instance to check defaults
        var dialog = new RoommateRequestDialog();
        
        // OtherUserName should be empty string by default (based on component code)
        dialog.OtherUserName.Should().BeEmpty();
        // IsConfirmation should default to false
        dialog.IsConfirmation.Should().BeFalse();
    }

    [Fact]
    public void RoommateRequestDialog_ComponentType_IsCorrect()
    {
        // Verify the component inherits from ComponentBase
        typeof(RoommateRequestDialog).Should().BeAssignableTo<ComponentBase>();
    }
}
