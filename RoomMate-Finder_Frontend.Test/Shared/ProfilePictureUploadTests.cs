using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Shared;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Shared;

public class ProfilePictureUploadTests : BunitContext, IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;
    public new async Task DisposeAsync() => await base.DisposeAsync();
    public ProfilePictureUploadTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
        
        // Mock common JS functions used by the component
        JSInterop.SetupVoid("preventScrollOnElement", _ => true);
        JSInterop.SetupVoid("allowScrollOnElement", _ => true);
    }

    [Fact]
    public void ProfilePictureUpload_Renders_UploadButtonInitially()
    {
        var cut = Render<ProfilePictureUpload>();

        cut.Markup.Should().Contain("Alege Poză");
        cut.FindComponent<MudIcon>().Instance.Icon.Should().Be(Icons.Material.Filled.AddAPhoto);
    }

    [Fact]
    public void ProfilePictureUpload_WithPreviewUrl_RendersExistingImage()
    {
        var previewUrl = "http://example.com/image.jpg";
        var cut = Render<ProfilePictureUpload>(parameters => parameters
            .Add(p => p.PreviewUrl, previewUrl));

        cut.Markup.Should().Contain(previewUrl);
        cut.Markup.Should().Contain("Poză actuală");
        cut.Markup.Should().Contain("Schimbă poza");
    }

    [Fact]
    public void HandleFileSelected_ValidImage_UpdatesState()
    {
        var cut = Render<ProfilePictureUpload>();
        
        // Create a mock IBrowserFile
        var file = InputFileContent.CreateFromText("dummy image content", "test.jpg", contentType: "image/jpeg");
        
        // Find InputFile and trigger change
        var inputFile = cut.FindComponent<InputFile>();
        inputFile.UploadFiles(file);

        // Verify state changes
        cut.Instance.HasNewImage.Should().BeTrue();
    }

    [Fact]
    public void HandleFileSelected_InvokesCallback()
    {
        IBrowserFile? selectedFile = null;
        var cut = Render<ProfilePictureUpload>(parameters => parameters
            .Add(p => p.OnFileSelected, file => selectedFile = file));

        var file = InputFileContent.CreateFromText("content", "test.png", contentType: "image/png");
        var inputFile = cut.FindComponent<InputFile>();
        
        inputFile.UploadFiles(file);

        selectedFile.Should().NotBeNull();
        selectedFile!.Name.Should().Be("test.png");
    }

    [Fact]
    public void CropControls_Render_WhenNewImageAndUrlPresent()
    {
        // Simulate state where new image is selected and URL is updated by parent
        var cut = Render<ProfilePictureUpload>(parameters => parameters
            .Add(p => p.PreviewUrl, "blob:url"));

        // Trigger file selection to set _hasNewImage = true
        var file = InputFileContent.CreateFromText("content", "new.jpg", contentType: "image/jpeg");
        cut.FindComponent<InputFile>().UploadFiles(file);
        
        cut.Render(); // Ensure render cycle

        cut.Markup.Should().Contain("crop-container");
        cut.Markup.Should().Contain("Resetează");
        cut.FindComponents<MudSlider<double>>().Should().HaveCount(2); // Zoom and Radius
    }

    [Fact]
    public async Task GetCroppedImageAsync_CallsJSAndReturnsBytes()
    {
        var cut = Render<ProfilePictureUpload>(parameters => parameters
            .Add(p => p.PreviewUrl, "blob:url"));

        // Set internal state to allow cropping
        var file = InputFileContent.CreateFromText("content", "new.jpg", contentType: "image/jpeg");
        cut.FindComponent<InputFile>().UploadFiles(file);

        // Mock JS return
        var base64 = "base64,SGVsbG8gV29ybGQ="; // "Hello World"
        var expectedBytes = Convert.FromBase64String("SGVsbG8gV29ybGQ=");

        JSInterop.Setup<string>("cropImageToCircle", _ => true)
            .SetResult(base64);

        // Act
        var result = await cut.Instance.GetCroppedImageAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedBytes);
        
        // Verify JS call arguments
        JSInterop.VerifyInvoke("cropImageToCircle", 1);
    }

    [Fact]
    public async Task GetCroppedImageAsync_ReturnsNull_IfNoNewImage()
    {
        var cut = Render<ProfilePictureUpload>(parameters => parameters
            .Add(p => p.PreviewUrl, "http://existing.com/img.jpg"));

        // _hasNewImage is false by default
        
        var result = await cut.Instance.GetCroppedImageAsync();

        result.Should().BeNull();
        JSInterop.VerifyNotInvoke("cropImageToCircle");
    }

    [Fact]
    public void ResetCrop_ResetsValues()
    {
        var cut = Render<ProfilePictureUpload>(parameters => parameters
            .Add(p => p.PreviewUrl, "blob:url"));

        var file = InputFileContent.CreateFromText("content", "new.jpg", contentType: "image/jpeg");
        cut.FindComponent<InputFile>().UploadFiles(file);

        var resetBtn = cut.FindComponents<MudButton>()
            .First(b => b.Markup.Contains("Resetează"));
            
        resetBtn.Find("button").Click();

        // Assert
        // Assert
        cut.Instance.HasNewImage.Should().BeTrue();
        cut.Markup.Should().Contain("Zoom: 1.0x");
    }
}
