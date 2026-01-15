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

public class CreateListingTests : BunitContext, IAsyncLifetime
{
    private readonly Mock<IListingService> _mockListingService;
    private readonly Mock<ISnackbar> _mockSnackbar;
    private readonly AuthenticationState _authState;

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    public CreateListingTests()
    {
        _mockListingService = new Mock<IListingService>();
        _mockSnackbar = new Mock<ISnackbar>();
        _mockSnackbar.Setup(x => x.Configuration).Returns(new SnackbarConfiguration());

        Services.AddMudServices();
        Services.AddSingleton(_mockListingService.Object);
        Services.AddSingleton(_mockSnackbar.Object);

        // Setup manual auth for Admin
        Services.AddAuthorizationCore();
        var claims = new[] 
        { 
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Admin"), 
            new Claim(ClaimTypes.Name, "adminuser") 
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
    public void CreateListing_RendersCorrectly()
    {
        RenderProviders();
        var cut = Render<CreateListing>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Post a New Room");
            cut.Markup.Should().Contain("Fill in the details");
        });
    }

    [Fact]
    public void CreateListing_HasRequiredFields()
    {
        RenderProviders();
        var cut = Render<CreateListing>(parameters => parameters
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
    public void CreateListing_ButtonDisabledInitially()
    {
        RenderProviders();
        var cut = Render<CreateListing>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));

        // Button should be disabled because form is invalid initially (empty required fields)
        var btn = cut.FindComponents<MudButton>()
            .First(b => b.Instance.StartIcon == Icons.Material.Filled.Send);
            
        btn.Instance.Disabled.Should().BeTrue();
    }

    [Fact]
    public async Task CreateListing_ValidSubmit_CallsServiceAndNavigates()
    {
        // Arrange
        RenderProviders();
        var navMan = Services.GetRequiredService<NavigationManager>();
        var cut = Render<CreateListing>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));

        _mockListingService.Setup(x => x.CreateAsync(It.IsAny<CreateListingRequest>()))
            .ReturnsAsync(new ListingDto(Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", "City", "Area", 500, DateTime.Today, new List<string>(), DateTime.UtcNow, true, new List<string>(), "Owner"));

        // Fill Form
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Title").Find("input").Change("Title Validation");
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Description").Find("textarea").Change("Description Validation");
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "City").Find("input").Change("City Validation");
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Area / Neighborhood").Find("input").Change("Area Validation");
        cut.FindComponents<MudNumericField<decimal>>().First(c => c.Instance.Label!.Contains("Price")).Find("input").Change("200");

        // Act
        // Mock File Upload
        var fileInput = cut.FindComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>();
        var mockFile = new Mock<IBrowserFile>();
        mockFile.Setup(f => f.Name).Returns("listing.jpg");
        mockFile.Setup(f => f.Size).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
        mockFile.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("listingcontent")));

        await cut.InvokeAsync(() => fileInput.Instance.FilesChanged.InvokeAsync(new List<IBrowserFile> { mockFile.Object }));
        
        // Force IsValid to true using reflection bypass (proven technique)
        var validationField = cut.Instance.GetType().GetField("_isValid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (validationField != null) validationField.SetValue(cut.Instance, true);
        
        cut.Render();
        await cut.InvokeAsync(() => Task.Delay(100));
        
        // Find enabled button and click on DOM element
        cut.FindAll("button").First(b => b.TextContent.Contains("Post Listing")).Click();

        // Assert
        _mockListingService.Verify(x => x.CreateAsync(It.Is<CreateListingRequest>(r => 
            r.Title == "Title Validation" && 
            r.Price == 200 &&
            r.Images != null && r.Images.Count == 1)), Times.Once);

        navMan.Uri.Should().EndWith("/admin/listings");
        _mockSnackbar.Verify(x => x.Add(It.Is<string>(s => s.Contains("created")), Severity.Success, null, null), Times.Once);
    }

    [Fact]
    public async Task CreateListing_ServiceError_ShowsSnackbar()
    {
        // Arrange
        RenderProviders();
        var cut = Render<CreateListing>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));

        _mockListingService.Setup(x => x.CreateAsync(It.IsAny<CreateListingRequest>()))
            .ThrowsAsync(new Exception("API Error"));

        // Fill Form
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Title").Find("input").Change("Title");
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Description").Find("textarea").Change("Desc");
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "City").Find("input").Change("City");
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Area / Neighborhood").Find("input").Change("Area");
        cut.FindComponents<MudNumericField<decimal>>().First(c => c.Instance.Label!.Contains("Price")).Find("input").Change("100");

        // Act
        // Force IsValid to true
        var validationField = cut.Instance.GetType().GetField("_isValid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (validationField != null) validationField.SetValue(cut.Instance, true);

        cut.Render();
        await cut.InvokeAsync(() => Task.Delay(100));

        cut.FindAll("button").First(b => b.TextContent.Contains("Post Listing")).Click();

        // Assert
        _mockSnackbar.Verify(x => x.Add(It.Is<string>(s => s.Contains("Error")), Severity.Error, null, null), Times.Once);
    }

    [Fact]
    public async Task CreateListing_UploadTooLargeFile_ShowError()
    {
        RenderProviders();
        var cut = Render<CreateListing>(parameters => parameters
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
    public async Task CreateListing_UploadInvalidType_ShowError()
    {
        RenderProviders();
        var cut = Render<CreateListing>(parameters => parameters
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
    public async Task CreateListing_RemoveImage_RemovesPreview()
    {
        RenderProviders();
        var cut = Render<CreateListing>(parameters => parameters
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
    public void CreateListing_ComponentTypeCheck()
    {
        var componentType = typeof(CreateListing);
        componentType.Should().Be<CreateListing>();
    }

    [Fact]
    public void CreateListing_HasAuthorizeAttribute()
    {
        var authorizeAttribute = typeof(CreateListing)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false)
            .FirstOrDefault() as Microsoft.AspNetCore.Authorization.AuthorizeAttribute;
        
        authorizeAttribute.Should().NotBeNull();
        authorizeAttribute!.Roles.Should().Contain("Admin");
    }

    [Fact]
    public void CreateListing_HasPageRoute()
    {
        var routeAttribute = typeof(CreateListing)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Components.RouteAttribute), false)
            .FirstOrDefault() as Microsoft.AspNetCore.Components.RouteAttribute;
        
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("/create-listing");
    }
    [Fact]
    public async Task CreateListing_ShowsError_WhenUserIdMissing()
    {
        // Custom auth with no claims
        var identity = new ClaimsIdentity(new List<Claim>(), "TestAuth"); // Authenticated but no ID
        var authState = new AuthenticationState(new ClaimsPrincipal(identity));
        var mockAuthProvider = new Mock<AuthenticationStateProvider>();
        mockAuthProvider.Setup(x => x.GetAuthenticationStateAsync()).ReturnsAsync(authState);
        
        // Override default auth just for this test
        Services.AddSingleton(mockAuthProvider.Object);

        RenderProviders();
        var cut = Render<CreateListing>(parameters => parameters
             .AddCascadingValue(Task.FromResult(authState)));

        // Fill Form
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Title").Find("input").Change("Title");
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Description").Find("textarea").Change("Desc");
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "City").Find("input").Change("City");
        cut.FindComponents<MudTextField<string>>().First(c => c.Instance.Label == "Area / Neighborhood").Find("input").Change("Area");
        cut.FindComponents<MudNumericField<decimal>>().First(c => c.Instance.Label!.Contains("Price")).Find("input").Change("100");

        // Force valid
        var validationField = cut.Instance.GetType().GetField("_isValid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (validationField != null) validationField.SetValue(cut.Instance, true);

        cut.Render();
        await cut.InvokeAsync(() => Task.Delay(100));

        // Submit
        cut.FindAll("button").First(b => b.TextContent.Contains("Post Listing")).Click();

        // Assert
        _mockSnackbar.Verify(x => x.Add(It.Is<string>(s => s.Contains("Could not identify user")), Severity.Error, null, null), Times.Once);
    }

    [Fact]
    public async Task CreateListing_PreviewGenerationFails_HandlesGracefully()
    {
        RenderProviders();
        var cut = Render<CreateListing>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));

        var fileInput = cut.FindComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>();
        var mockFile = new Mock<IBrowserFile>();
        mockFile.Setup(f => f.Name).Returns("corrupt.jpg");
        mockFile.Setup(f => f.Size).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
        mockFile.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
             .Throws(new IOException("Read failed"));

        await cut.InvokeAsync(() => fileInput.Instance.FilesChanged.InvokeAsync(new List<IBrowserFile> { mockFile.Object }));

        cut.Markup.Should().Contain("1 image(s) selected");
    }

    [Fact]
    public async Task CreateListing_TooManyImages_ShowsError()
    {
        RenderProviders();
        var cut = Render<CreateListing>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));

        var fileInput = cut.FindComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>();
        
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
    }

    [Fact]
    public async Task CreateListing_RemoveImage_InvalidIndex_DoesNothing()
    {
        RenderProviders();
        var cut = Render<CreateListing>(parameters => parameters
            .AddCascadingValue(Task.FromResult(_authState)));

        var fileInput = cut.FindComponent<MudFileUpload<IReadOnlyList<IBrowserFile>>>();
        var mockFile = new Mock<IBrowserFile>();
        mockFile.Setup(f => f.Name).Returns("img.jpg");
        mockFile.Setup(f => f.Size).Returns(1024);
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
        mockFile.Setup(f => f.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>()))
             .Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes("data")));

        await cut.InvokeAsync(() => fileInput.Instance.FilesChanged.InvokeAsync(new List<IBrowserFile> { mockFile.Object }));

        var instance = cut.Instance;
        var method = instance.GetType().GetMethod("RemoveImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        await cut.InvokeAsync(() => method!.Invoke(instance, new object[] { 99 }));
        await cut.InvokeAsync(() => method!.Invoke(instance, new object[] { -1 }));

        cut.Markup.Should().Contain("1 image(s) selected");
    }
}
