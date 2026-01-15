using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Shared;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Shared;

public class RoommateRequestDialogTests : IAsyncLifetime
{
    private readonly TestContext _ctx = new();
    private IDialogService _dialogService = null!;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _ctx.DisposeAsync();
    }

    public RoommateRequestDialogTests()
    {
        _ctx.Services.AddMudServices(options =>
        {
            options.SnackbarConfiguration.ShowTransitionDuration = 0;
            options.SnackbarConfiguration.HideTransitionDuration = 0;
            options.SnackbarConfiguration.VisibleStateDuration = 0;
        });
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _dialogService = _ctx.Services.GetRequiredService<IDialogService>();
    }

    private IRenderedComponent<MudDialogProvider> RenderHeader()
    {
        _ctx.Render<MudPopoverProvider>();
        var cut = _ctx.Render<MudDialogProvider>();
        return cut;
    }

    [Fact]
    public void RoommateRequestDialog_NonConfirmation_OpensAndShowsContent()
    {
        // Arrange
        var cut = RenderHeader();
        var parameters = new DialogParameters
        {
            { nameof(RoommateRequestDialog.OtherUserName), "John Doe" },
            { nameof(RoommateRequestDialog.IsConfirmation), false }
        };

        // Act
        _dialogService.Show<RoommateRequestDialog>("Request", parameters);

        // Assert
        cut.WaitForState(() => cut.FindComponents<RoommateRequestDialog>().Count > 0);
        
        var dialog = cut.FindComponent<RoommateRequestDialog>();
        dialog.Markup.Should().Contain("Trimite o cerere de colegiat");
        dialog.Markup.Should().Contain("John Doe");
        dialog.Markup.Should().Contain("Trimite cererea");
    }

    [Fact]
    public void RoommateRequestDialog_Confirmation_OpensAndShowsContent()
    {
        // Arrange
        var cut = RenderHeader();
        var parameters = new DialogParameters
        {
            { nameof(RoommateRequestDialog.OtherUserName), "Jane Doe" },
            { nameof(RoommateRequestDialog.IsConfirmation), true }
        };

        // Act
        _dialogService.Show<RoommateRequestDialog>("Confirmation", parameters);

        // Assert
        cut.WaitForState(() => cut.FindComponents<RoommateRequestDialog>().Count > 0);
        
        var dialog = cut.FindComponent<RoommateRequestDialog>();
        dialog.Markup.Should().Contain("ți-a trimis deja o cerere");
        dialog.Markup.Should().Contain("Confirmă cererea");
        dialog.Markup.Should().Contain("Anulează");
    }

    [Fact]
    public async Task RoommateRequestDialog_Cancel_ClosesDialogWithCancel()
    {
        // Arrange
        var cut = RenderHeader();
        var parameters = new DialogParameters
        {
            { nameof(RoommateRequestDialog.OtherUserName), "John Doe" },
            { nameof(RoommateRequestDialog.IsConfirmation), false }
        };

        // Act
        var dlgRef = _dialogService.Show<RoommateRequestDialog>("Request", parameters);
        cut.WaitForState(() => cut.FindComponents<RoommateRequestDialog>().Count > 0);
        var dialog = cut.FindComponent<RoommateRequestDialog>();

        // Find Cancel button (Color.Default)
        // Or by text
        var btn = dialog.FindComponents<MudButton>().FirstOrDefault(x => x.Markup.Contains("Anulează"));
        btn.Should().NotBeNull();
        
        btn!.Find("button").Click();
        
        var result = await dlgRef.Result;
        
        // Assert
        result.Canceled.Should().BeTrue();
        
        // Dialog should disappear
        cut.WaitForState(() => cut.FindComponents<RoommateRequestDialog>().Count == 0);
    }

    [Fact]
    public async Task RoommateRequestDialog_Submit_ClosesDialogWithResult()
    {
        // Arrange
        var cut = RenderHeader();
        var parameters = new DialogParameters
        {
            { nameof(RoommateRequestDialog.OtherUserName), "John Doe" },
            { nameof(RoommateRequestDialog.IsConfirmation), false }
        };

        // Act
        var dlgRef = _dialogService.Show<RoommateRequestDialog>("Request", parameters);
        cut.WaitForState(() => cut.FindComponents<RoommateRequestDialog>().Count > 0);
        var dialog = cut.FindComponent<RoommateRequestDialog>();

        // Set message
        var tf = dialog.FindComponent<MudTextField<string>>();
        await cut.InvokeAsync(() => tf.Instance.ValueChanged.InvokeAsync("Hello!"));

        // Click Submit
        var btn = dialog.FindComponents<MudButton>().FirstOrDefault(x => x.Markup.Contains("Trimite cererea"));
        btn.Should().NotBeNull();
        
        btn!.Find("button").Click();

        var result = await dlgRef.Result;

        // Assert
        result.Canceled.Should().BeFalse();
        result.Data.Should().Be("Hello!");
        
        // Dialog should disappear
        cut.WaitForState(() => cut.FindComponents<RoommateRequestDialog>().Count == 0);
    }
    
    [Fact]
    public async Task RoommateRequestDialog_Confirmation_Submit_ClosesDialogWithResult()
    {
        // Arrange
        var cut = RenderHeader();
        var parameters = new DialogParameters
        {
            { nameof(RoommateRequestDialog.OtherUserName), "Jane Doe" },
            { nameof(RoommateRequestDialog.IsConfirmation), true }
        };

        // Act
        var dlgRef = _dialogService.Show<RoommateRequestDialog>("Confirmation", parameters);
        cut.WaitForState(() => cut.FindComponents<RoommateRequestDialog>().Count > 0);
        var dialog = cut.FindComponent<RoommateRequestDialog>();

        // Click Confirm
        var btn = dialog.FindComponents<MudButton>().FirstOrDefault(x => x.Markup.Contains("Confirmă cererea"));
        btn.Should().NotBeNull();
        
        btn!.Find("button").Click();

        var result = await dlgRef.Result;

        // Assert
        result.Canceled.Should().BeFalse();
        // Result data might be null or message depending on logic.
        // Component logic: MudDialog.Close(DialogResult.Ok(_message));
        // If _message is null (default), Data is null.
        result.Data.Should().BeNull();
        
        // Verify Dialog closed
        cut.WaitForState(() => cut.FindComponents<RoommateRequestDialog>().Count == 0);
    }
}
