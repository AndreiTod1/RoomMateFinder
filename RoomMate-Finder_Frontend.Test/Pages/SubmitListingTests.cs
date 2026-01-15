using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;
using System.Security.Claims;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class SubmitListingTests : BunitContext, IAsyncLifetime
{
    private readonly Mock<IListingService> _mockListingService;
    private readonly Mock<ISnackbar> _mockSnackbar;
    private readonly AuthenticationState _authState;

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    public SubmitListingTests()
    {
        _mockListingService = new Mock<IListingService>();
        _mockSnackbar = new Mock<ISnackbar>();
        _mockSnackbar.Setup(x => x.Configuration).Returns(new SnackbarConfiguration());

        Services.AddMudServices();
        Services.AddSingleton(_mockListingService.Object);
        Services.AddSingleton(_mockSnackbar.Object);

        // Setup manual auth for User
        Services.AddAuthorizationCore();
        var claims = new[] 
        { 
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "User"), 
            new Claim(ClaimTypes.Name, "testuser") 
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        _authState = new AuthenticationState(user);

        var mockAuthProvider = new Mock<AuthenticationStateProvider>();
        mockAuthProvider.Setup(x => x.GetAuthenticationStateAsync()).ReturnsAsync(_authState);
        Services.AddSingleton(mockAuthProvider.Object);
        Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();

        JSInterop.Mode = JSRuntimeMode.Loose;
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
    }

    private void RenderProviders()
    {
        Render<MudPopoverProvider>();
        Render<MudDialogProvider>();
        Render<MudSnackbarProvider>();
    }

    [Fact]
    public void SubmitListing_RendersCorrectly()
    {
        RenderProviders();
        var cut = Render<SubmitListing>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Submit a New Room");
            cut.Markup.Should().Contain("Fill in the details");
        });
    }

    [Fact]
    public void SubmitListing_HasRequiredInputs()
    {
        RenderProviders();
        var cut = Render<SubmitListing>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Title");
            cut.Markup.Should().Contain("Description");
            cut.Markup.Should().Contain("City");
            cut.Markup.Should().Contain("Area");
            cut.Markup.Should().Contain("Price");
            cut.Markup.Should().Contain("Available From");
        });
    }

    [Fact]
    public void SubmitListing_ButtonDisabledInitially()
    {
        RenderProviders();
        var cut = Render<SubmitListing>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));

        var btn = cut.FindComponents<MudButton>()
            .First(b => b.Instance.StartIcon == Icons.Material.Filled.Send);
            
        btn.Instance.Disabled.Should().BeTrue();
    }

    [Fact]
    public async Task SubmitListing_ValidSubmit_CallsServiceAndNavigates()
    {
        // Arrange
        RenderProviders();
        var navMan = Services.GetRequiredService<NavigationManager>();
        var cut = Render<SubmitListing>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));

        _mockListingService.Setup(x => x.CreateAsync(It.IsAny<CreateListingRequest>()))
            .ReturnsAsync(new ListingDto(Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", "City", "Area", 500, DateTime.Today, new List<string>(), DateTime.UtcNow, true, new List<string>(), "Owner"));

        // Fill Form
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Title").Find("input").Change("Cozy Room");
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Description").Find("textarea").Change("Great room description.");
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "City").Find("input").Change("Bucharest");
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Area / Neighborhood").Find("input").Change("Center");
        cut.FindComponents<MudNumericField<decimal>>().First(c => c.Instance.Label.Contains("Price")).Find("input").Change("450");
        
        // Mock File Upload
        var fileInput = cut.FindComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>();
        var mockFile = new Mock<IBrowserFile>();
        mockFile.Setup(f => f.Name).Returns("room.jpg");
        mockFile.Setup(f => f.Size).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
        mockFile.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("imagecontent")));

        await cut.InvokeAsync(() => fileInput.Instance.FilesChanged.InvokeAsync(new List<IBrowserFile> { mockFile.Object }));

        // Act
        // Bypass validation strictness if needed, or rely on form valid state
        var validationField = cut.Instance.GetType().GetField("_isValid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (validationField != null) validationField.SetValue(cut.Instance, true);

        cut.Render();
        await cut.InvokeAsync(() => Task.Delay(100));

        // Submit
        cut.FindAll("button").First(b => b.TextContent.Contains("Submit for Approval")).Click();

        // Assert
        _mockListingService.Verify(x => x.CreateAsync(It.Is<CreateListingRequest>(r => 
            r.Title == "Cozy Room" && 
            r.Images != null && r.Images.Count == 1)), Times.Once);

        navMan.Uri.Should().EndWith("/my-listings");
        _mockSnackbar.Verify(x => x.Add(It.Is<string>(s => s.Contains("submitted")), Severity.Success, null, null), Times.Once);
    }

    [Fact]
    public async Task SubmitListing_ServiceError_ShowsSnackbar()
    {
        // Arrange
        RenderProviders();
        var cut = Render<SubmitListing>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));

        _mockListingService.Setup(x => x.CreateAsync(It.IsAny<CreateListingRequest>()))
            .ThrowsAsync(new Exception("Submission Failed"));

        // Fill Form
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Title").Find("input").Change("Title");
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Description").Find("textarea").Change("Desc");
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "City").Find("input").Change("City");
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Area / Neighborhood").Find("input").Change("Area");
        cut.FindComponents<MudNumericField<decimal>>().First(c => c.Instance.Label.Contains("Price")).Find("input").Change("100");

        var validationField = cut.Instance.GetType().GetField("_isValid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (validationField != null) validationField.SetValue(cut.Instance, true);

        cut.Render();
        await cut.InvokeAsync(() => Task.Delay(100));

        cut.FindAll("button").First(b => b.TextContent.Contains("Submit for Approval")).Click();

        // Assert
        _mockSnackbar.Verify(x => x.Add(It.Is<string>(s => s.Contains("Error")), Severity.Error, null, null), Times.Once);
    }

    [Fact]
    public async Task SubmitListing_UploadTooLargeFile_ShowError()
    {
        RenderProviders();
        var cut = Render<SubmitListing>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));

        var fileInput = cut.FindComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>();
        var mockFile = new Mock<IBrowserFile>();
        mockFile.Setup(f => f.Name).Returns("huge.jpg");
        mockFile.Setup(f => f.Size).Returns(10 * 1024 * 1024); // 10MB
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");

        await cut.InvokeAsync(() => fileInput.Instance.FilesChanged.InvokeAsync(new List<IBrowserFile> { mockFile.Object }));

        cut.Markup.Should().Contain("File too large");
    }

    [Fact]
    public async Task SubmitListing_UploadInvalidType_ShowError()
    {
        RenderProviders();
        var cut = Render<SubmitListing>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));

        var fileInput = cut.FindComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>();
        var mockFile = new Mock<IBrowserFile>();
        mockFile.Setup(f => f.Name).Returns("virus.exe");
        mockFile.Setup(f => f.Size).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("application/exe");

        await cut.InvokeAsync(() => fileInput.Instance.FilesChanged.InvokeAsync(new List<IBrowserFile> { mockFile.Object }));

        cut.Markup.Should().Contain("Invalid file type");
    }

    [Fact]
    public async Task SubmitListing_RemoveImage_RemovesPreview()
    {
        RenderProviders();
        var cut = Render<SubmitListing>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));

        // Add file
        var fileInput = cut.FindComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>();
        var mockFile = new Mock<IBrowserFile>();
        mockFile.Setup(f => f.Name).Returns("image.jpg");
        mockFile.Setup(f => f.Size).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
        mockFile.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
             .Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("data")));

        await cut.InvokeAsync(() => fileInput.Instance.FilesChanged.InvokeAsync(new List<IBrowserFile> { mockFile.Object }));
        
        // Find remove button and click
        var removeBtn = cut.Find("button[class*='mud-icon-button']"); // Approximate selector for the X button
        removeBtn.Click();

        cut.Markup.Should().NotContain("image.jpg");
    }

    [Fact]
    public async Task SubmitListing_RedirectsToLogin_WhenUserIdMissing()
    {
        // Custom auth with no claims
        var identity = new ClaimsIdentity(new List<Claim>(), "TestAuth"); // Authenticated but no ID
        var authState = new AuthenticationState(new ClaimsPrincipal(identity));
        var mockAuthProvider = new Mock<AuthenticationStateProvider>();
        mockAuthProvider.Setup(x => x.GetAuthenticationStateAsync()).ReturnsAsync(authState);
        
        // Override default auth just for this test
        Services.AddSingleton(mockAuthProvider.Object);
        var navMan = Services.GetRequiredService<NavigationManager>();

        RenderProviders();
        var cut = Render<SubmitListing>(parameters => parameters
             .AddCascadingValue(Task.FromResult(authState)));

        cut.WaitForAssertion(() => 
        {
            navMan.Uri.Should().EndWith("/login");
            _mockSnackbar.Verify(x => x.Add(It.Is<string>(s => s.Contains("Unable to identify")), Severity.Error, null, null), Times.Once);
        });
    }

    [Fact]
    public async Task SubmitListing_PreviewGenerationFails_HandlesGracefully()
    {
        RenderProviders();
        var cut = Render<SubmitListing>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));

        var fileInput = cut.FindComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>();
        var mockFile = new Mock<IBrowserFile>();
        mockFile.Setup(f => f.Name).Returns("corrupt.jpg");
        mockFile.Setup(f => f.Size).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
        mockFile.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
             .Throws(new IOException("Read failed")); // Simulate stream error

        // Should not throw
        await cut.InvokeAsync(() => fileInput.Instance.FilesChanged.InvokeAsync(new List<IBrowserFile> { mockFile.Object }));

        // Image should be added to list even if preview failed (based on current implementation 'catch' block swallowing error)
        // Or if the implementation swallows it, at least we shouldn't crash.
        // The current implementation adds to _selectedImages BEFORE the try/catch for preview.
        // So checking the "images selected" text count is a good way to verify it was added.
        cut.Markup.Should().Contain("1 image(s) selected");
    }

    [Fact]
    public async Task SubmitListing_TooManyImages_ShowsError()
    {
        RenderProviders();
        var cut = Render<SubmitListing>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));

        var fileInput = cut.FindComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>();
        
        // Create 9 dummy files
        var files = Enumerable.Range(0, 9).Select(i => 
        {
            var m = new Mock<IBrowserFile>();
            m.Setup(f => f.Name).Returns($"img{i}.jpg");
            m.Setup(f => f.Size).Returns(1024);
            m.Setup(f => f.ContentType).Returns("image/jpeg");
            return m.Object;
        }).ToList();

        await cut.InvokeAsync(() => fileInput.Instance.FilesChanged.InvokeAsync(files));

        cut.Markup.Should().Contain("Maximum 8 images allowed");
        cut.Markup.Should().NotContain("image(s) selected"); // Should reject valid ones too if batch is too big? Implementation says: return if total > 8
    }

    [Fact]
    public async Task SubmitListing_RemoveImage_InvalidIndex_DoesNothing()
    {
        RenderProviders();
        var cut = Render<SubmitListing>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));

        // Try removing index 0 when empty
        // Need to invoke the method directly or finding a button that doesn't exist?
        // Since the buttons only render if images exist, we must use reflection or similar to 'force' the call
        // OR add an image first, then try removing index 5 (out of bounds)
        
        var fileInput = cut.FindComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>();
        var mockFile = new Mock<IBrowserFile>();
        mockFile.Setup(f => f.Name).Returns("img.jpg");
        mockFile.Setup(f => f.Size).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
        mockFile.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
             .Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("data")));

        await cut.InvokeAsync(() => fileInput.Instance.FilesChanged.InvokeAsync(new List<IBrowserFile> { mockFile.Object }));

        // Now we have 1 image at index 0.
        // Call RemoveImage via reflection or if we can trigger it. 
        // Reflection is easiest to force the bad index logic coverage.
        var instance = cut.Instance;
        var method = instance.GetType().GetMethod("RemoveImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        // Act - Remove invalid index
        await cut.InvokeAsync(() => method.Invoke(instance, new object[] { 99 }));
        await cut.InvokeAsync(() => method.Invoke(instance, new object[] { -1 }));

        // Assert - still has 1 image
        cut.Markup.Should().Contain("1 image(s) selected");
    }

    [Fact]
    public async Task SubmitListing_AddMultipleImages_RendersCorrectly()
    {
        RenderProviders();
        var cut = Render<SubmitListing>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));

        var fileInput = cut.FindComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>();
        
        // Add 3 images
        var files = Enumerable.Range(0, 3).Select(i => 
        {
            var m = new Mock<IBrowserFile>();
            m.Setup(f => f.Name).Returns($"img{i}.jpg");
            m.Setup(f => f.Size).Returns(1024);
            m.Setup(f => f.ContentType).Returns("image/jpeg");
            m.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
             .Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("data")));
            return m.Object;
        }).ToList();

        await cut.InvokeAsync(() => fileInput.Instance.FilesChanged.InvokeAsync(files));
        
        // Verify 3 images are added
        cut.Markup.Should().Contain("3 image(s) selected");
    }

    [Fact]
    public async Task SubmitListing_ParsesAmenitiesCorrectly()
    {
        RenderProviders();
        var cut = Render<SubmitListing>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));

        _mockListingService.Setup(x => x.CreateAsync(It.IsAny<CreateListingRequest>()))
            .ReturnsAsync(new ListingDto(Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", "City", "Area", 500, DateTime.Today, new List<string>(), DateTime.UtcNow, true, new List<string>(), "Owner"));

        // Fill Form
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Title").Find("input").Change("Title");
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Description").Find("textarea").Change("Desc");
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "City").Find("input").Change("City");
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Area / Neighborhood").Find("input").Change("Area");
        cut.FindComponents<MudNumericField<decimal>>().First(c => c.Instance.Label.Contains("Price")).Find("input").Change("100");
        
        // Amenities with spacing and empty entries
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label.Contains("Amenities")).Find("input").Change(" WiFi , , Balcony,  Parking ");

        var validationField = cut.Instance.GetType().GetField("_isValid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (validationField != null) validationField.SetValue(cut.Instance, true);

        cut.Render();
        await cut.InvokeAsync(() => Task.Delay(100));

        cut.FindAll("button").First(b => b.TextContent.Contains("Submit for Approval")).Click();

        _mockListingService.Verify(x => x.CreateAsync(It.Is<CreateListingRequest>(r => 
            r.Amenities.Count == 3 &&
            r.Amenities.Contains("WiFi") &&
            r.Amenities.Contains("Balcony") &&
            r.Amenities.Contains("Parking")
        )), Times.Once);
    }
}
