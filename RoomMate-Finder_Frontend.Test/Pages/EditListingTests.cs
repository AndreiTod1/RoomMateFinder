using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using Microsoft.Extensions.Configuration;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;
using System.Security.Claims;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class EditListingTests : BunitContext, IAsyncLifetime
{
    private readonly Mock<IListingService> _mockListingService;
    private readonly Mock<ISnackbar> _mockSnackbar;
    private readonly AuthenticationState _authState;

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    public EditListingTests()
    {
        _mockListingService = new Mock<IListingService>();
        _mockSnackbar = new Mock<ISnackbar>();

        Services.AddMudServices();
        Services.AddSingleton(_mockListingService.Object);
        Services.AddSingleton(_mockSnackbar.Object);

        // Setup manual auth
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

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private IRenderedComponent<EditListing> RenderComponent(Guid id)
    {
        return Render<EditListing>(parameters => parameters
            .Add(p => p.Id, id)
            .AddCascadingValue(Task.FromResult(_authState)));
    }
    
    private void RenderProviders()
    {
        Render<MudPopoverProvider>();
        Render<MudDialogProvider>();
        // MudSnackbarProvider omitted
    }

    [Fact]
    public void EditListing_Loading_ShowsProgressLinear()
    {
        // Arrange
        var tcs = new TaskCompletionSource<ListingDto?>();
        _mockListingService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).Returns(tcs.Task);
        
        RenderProviders();

        // Act
        var cut = RenderComponent(Guid.NewGuid());

        // Assert
        cut.FindComponents<MudProgressLinear>().Should().NotBeEmpty();

        // Cleanup
        tcs.SetResult(null);
    }
    
    [Fact]
    public void EditListing_NotFound_ShowsError()
    {
        // Arrange
        _mockListingService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ListingDto?)null);
        
        RenderProviders();

        // Act
        var cut = RenderComponent(Guid.NewGuid());

        // Assert
        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Listing not found"));
        cut.FindComponents<MudAlert>().Should().NotBeEmpty();
    }

    [Fact]
    public void EditListing_Found_PopulatesForm()
    {
        // Arrange
        var id = Guid.NewGuid();
        var listing = new ListingDto(
            id, Guid.NewGuid(), "My Title", "My Desc", "City", "Area", 500, DateTime.UtcNow, 
            new List<string> { "WiFi" }, DateTime.UtcNow, true
        );
        
        _mockListingService.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(listing);
        
        RenderProviders();

        // Act
        var cut = RenderComponent(id);

        // Assert
        cut.WaitForAssertion(() => cut.Find("input[type='text']").Should().NotBeNull());
        
        // Check assumption that title is populated (MudTextField value binding)
        // We can check the instance or markup
        cut.Markup.Should().Contain("My Title");
        cut.Markup.Should().Contain("City");
    }

    [Fact]
    public async Task EditListing_SaveChanges_CallsServiceAndRedirects()
    {
        // Arrange
        var id = Guid.NewGuid();
        var listing = new ListingDto(
            id, Guid.NewGuid(), "Old Title", "Old Desc", "Old City", "Old Area", 500, DateTime.Today, 
            new List<string> { "WiFi" }, DateTime.UtcNow, true
        );
        
        _mockListingService.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(listing);
        
        RenderProviders();

        var cut = RenderComponent(id);
        cut.WaitForAssertion(() => cut.Markup.Contains("Old Title"));

        // Act
        // Modify Title
        var titleField = cut.FindComponents<MudTextField<string>>().First(x => x.Instance.Label == "Title");
        await cut.InvokeAsync(() => titleField.Instance.ValueChanged.InvokeAsync("New Title"));
        
        // Find Save Button
        var saveBtn = cut.FindComponents<MudButton>().First(b => b.Markup.Contains("Save Changes"));
        
        // Force validation
        await cut.InvokeAsync(() => cut.FindComponent<MudForm>().Instance.Validate());

        // Wait for validation to enable button
        cut.WaitForState(() => !saveBtn.Instance.Disabled, TimeSpan.FromSeconds(2));

        // Click
        await cut.InvokeAsync(() => saveBtn.Find("button").Click());

        // Assert
        _mockListingService.Verify(x => x.UpdateAsync(id, It.Is<UpdateListingRequest>(r => 
            r.Title == "New Title" && 
            r.Description == "Old Desc"
        )), Times.Once);
        
        _mockSnackbar.Verify(x => x.Add(It.Is<string>(s => s.Contains("updated")), Severity.Success, null, null), Times.Once);
        
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.Uri.Should().EndWith("/admin/listings");
    }

    [Fact]
    public async Task EditListing_Amenities_Parsing_Works()
    {
        // Arrange
        var id = Guid.NewGuid();
        var listing = new ListingDto(
            id, Guid.NewGuid(), "Title", "Desc", "City", "Area", 500, DateTime.Today, 
            new List<string>(), DateTime.UtcNow, true
        );
        
        _mockListingService.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(listing);
        
        RenderProviders();
        var cut = RenderComponent(id);
        cut.WaitForAssertion(() => cut.Markup.Contains("Title"));

        // Enter Amenities
        var amenitiesField = cut.FindComponents<MudTextField<string>>().First(x => x.Instance.Label.Contains("Amenities"));
        await cut.InvokeAsync(() => amenitiesField.Instance.ValueChanged.InvokeAsync("WiFi, AC,  Parking "));

        // Save
        var saveBtn = cut.FindComponents<MudButton>().First(b => b.Markup.Contains("Save Changes"));
        
        await cut.InvokeAsync(() => cut.FindComponent<MudForm>().Instance.Validate());
        
        cut.WaitForState(() => !saveBtn.Instance.Disabled);
        await cut.InvokeAsync(() => saveBtn.Find("button").Click());

        // Assert
        _mockListingService.Verify(x => x.UpdateAsync(id, It.Is<UpdateListingRequest>(r => 
            r.Amenities.Count == 3 &&
            r.Amenities.Contains("WiFi") &&
            r.Amenities.Contains("AC") &&
            r.Amenities.Contains("Parking")
        )), Times.Once);
    }
    
    [Fact]
    public void EditListing_Cancel_RedirectsBack()
    {
        // Arrange
        var id = Guid.NewGuid();
        var listing = new ListingDto(
            id, Guid.NewGuid(), "Title", "Desc", "City", "Area", 500, DateTime.Today, 
            new List<string> { "WiFi" }, DateTime.UtcNow, true
        );
        
        _mockListingService.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(listing);
        
        RenderProviders();

        var cut = RenderComponent(id);
        cut.WaitForAssertion(() => cut.Markup.Contains("Title"));

        // Act
        var cancelBtn = cut.FindComponents<MudButton>().First(b => b.Markup.Contains("Cancel"));
        cancelBtn.Find("button").Click();
        
        // Assert
        var nav = Services.GetRequiredService<NavigationManager>(); // bUnit's fake
        nav.Uri.Should().EndWith("/admin/listings");
    }
}
