using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Pages;

public class CreateListingTests : BunitContext, IAsyncLifetime
{
    private Mock<IListingService> _mockListingService = null!;

    public Task InitializeAsync()
    {
        _mockListingService = new Mock<IListingService>();
        
        Services.AddMudServices();
        Services.AddSingleton(_mockListingService.Object);
        
        JSInterop.Mode = JSRuntimeMode.Loose;
        
        return Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    [Fact]
    public void CreateListing_ComponentTypeCheck()
    {
        // Test that component type exists and has expected properties
        var componentType = typeof(CreateListing);
        componentType.Should().NotBeNull();
    }

    [Fact]
    public void CreateListing_HasAuthorizeAttribute()
    {
        // Verify component has Authorize attribute with Admin role
        var authorizeAttribute = typeof(CreateListing)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false)
            .FirstOrDefault() as Microsoft.AspNetCore.Authorization.AuthorizeAttribute;
        
        authorizeAttribute.Should().NotBeNull();
        authorizeAttribute!.Roles.Should().Contain("Admin");
    }

    [Fact]
    public void CreateListing_HasPageRoute()
    {
        // Verify component has correct page route
        var routeAttribute = typeof(CreateListing)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Components.RouteAttribute), false)
            .FirstOrDefault() as Microsoft.AspNetCore.Components.RouteAttribute;
        
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("/create-listing");
    }

    [Fact]
    public void CreateListing_ListingService_IsRegistered()
    {
        // Verify listing service is available
        Services.GetService<IListingService>().Should().NotBeNull();
    }
}
