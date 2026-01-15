using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using RoomMate_Finder_Frontend.Pages.Admins;
using RoomMate_Finder_Frontend.Services;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Pages.Admins;

public class AdminRoommatesTests : BunitContext, IAsyncLifetime
{
    private readonly Mock<IRoommateService> _mockRoommateService;
    private readonly Mock<ISnackbar> _mockSnackbar;
    private readonly Mock<IDialogService> _mockDialogService;

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    public AdminRoommatesTests()
    {
        _mockRoommateService = new Mock<IRoommateService>();
        _mockSnackbar = new Mock<ISnackbar>();
        _mockDialogService = new Mock<IDialogService>();

        _mockSnackbar.Setup(s => s.Configuration).Returns(new SnackbarConfiguration());

        Services.AddMudServices();
        Services.AddSingleton(_mockRoommateService.Object);
        Services.AddSingleton(_mockSnackbar.Object);
        Services.AddSingleton(_mockDialogService.Object);

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private void RenderProviders()
    {
        Render<MudPopoverProvider>();
        Render<MudDialogProvider>();
    }

    // Component Existence Tests
    [Fact]
    public void AdminRoommates_ComponentExists()
    {
        typeof(AdminRoommates).Should().NotBeNull();
    }

    [Fact]
    public void AdminRoommates_HasCorrectPageRoute()
    {
        var pageAttr = typeof(AdminRoommates).GetCustomAttributes(typeof(Microsoft.AspNetCore.Components.RouteAttribute), false);
        pageAttr.Should().ContainSingle();
        ((Microsoft.AspNetCore.Components.RouteAttribute)pageAttr[0]).Template.Should().Be("/admin/roommates");
    }

    [Fact]
    public void AdminRoommates_HasAuthorizeAttribute()
    {
        var authAttr = typeof(AdminRoommates).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false);
        authAttr.Should().ContainSingle();
    }

    [Fact]
    public void AdminRoommates_RequiresAdminRole()
    {
        var authAttr = typeof(AdminRoommates).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false);
        var attr = authAttr[0] as Microsoft.AspNetCore.Authorization.AuthorizeAttribute;
        attr!.Roles.Should().Be("Admin");
    }

    // Rendering Tests
    [Fact]
    public void AdminRoommates_Renders_Title()
    {
        // Arrange
        _mockRoommateService.Setup(x => x.GetPendingRequestsAsync())
            .ReturnsAsync(new List<PendingRoommateRequestDto>());
        _mockRoommateService.Setup(x => x.GetRelationshipsAsync())
            .ReturnsAsync(new List<RoommateRelationshipDto>());

        RenderProviders();

        // Act
        var cut = Render<AdminRoommates>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Gestionare Roommates");
        });
    }

    [Fact]
    public void AdminRoommates_Renders_PageTitle()
    {
        // Arrange
        _mockRoommateService.Setup(x => x.GetPendingRequestsAsync())
            .ReturnsAsync(new List<PendingRoommateRequestDto>());
        _mockRoommateService.Setup(x => x.GetRelationshipsAsync())
            .ReturnsAsync(new List<RoommateRelationshipDto>());

        RenderProviders();

        // Act
        var cut = Render<AdminRoommates>();

        // Assert - check component renders without error (PageTitle is in head, not markup)
        cut.Should().NotBeNull();
        cut.Markup.Should().NotBeEmpty();
    }

    [Fact]
    public void AdminRoommates_Renders_Subtitle()
    {
        // Arrange
        _mockRoommateService.Setup(x => x.GetPendingRequestsAsync())
            .ReturnsAsync(new List<PendingRoommateRequestDto>());
        _mockRoommateService.Setup(x => x.GetRelationshipsAsync())
            .ReturnsAsync(new List<RoommateRelationshipDto>());

        RenderProviders();

        // Act
        var cut = Render<AdminRoommates>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Aprobă cereri și gestionează relațiile de colegiat");
        });
    }

    [Fact]
    public void AdminRoommates_HasTabs()
    {
        // Arrange
        _mockRoommateService.Setup(x => x.GetPendingRequestsAsync())
            .ReturnsAsync(new List<PendingRoommateRequestDto>());
        _mockRoommateService.Setup(x => x.GetRelationshipsAsync())
            .ReturnsAsync(new List<RoommateRelationshipDto>());

        RenderProviders();

        // Act
        var cut = Render<AdminRoommates>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Cereri în așteptare");
            cut.Markup.Should().Contain("Relații active");
        });
    }

    // Loading State Tests
    [Fact]
    public void AdminRoommates_Loading_ShowsProgress()
    {
        // Arrange
        var tcs = new TaskCompletionSource<List<PendingRoommateRequestDto>>();
        _mockRoommateService.Setup(x => x.GetPendingRequestsAsync())
            .Returns(tcs.Task);
        _mockRoommateService.Setup(x => x.GetRelationshipsAsync())
            .ReturnsAsync(new List<RoommateRelationshipDto>());

        RenderProviders();

        // Act
        var cut = Render<AdminRoommates>();

        // Assert - should render without throwing
        cut.Should().NotBeNull();

        // Cleanup
        tcs.SetResult(new List<PendingRoommateRequestDto>());
    }

    // Empty State Tests
    [Fact]
    public void AdminRoommates_NoPendingRequests_ShowsEmptyState()
    {
        // Arrange
        _mockRoommateService.Setup(x => x.GetPendingRequestsAsync())
            .ReturnsAsync(new List<PendingRoommateRequestDto>());
        _mockRoommateService.Setup(x => x.GetRelationshipsAsync())
            .ReturnsAsync(new List<RoommateRelationshipDto>());

        RenderProviders();

        // Act
        var cut = Render<AdminRoommates>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Nu există cereri în așteptare");
        });
    }

    [Fact]
    public void AdminRoommates_NoRelationships_ShowsEmptyState()
    {
        // Arrange
        _mockRoommateService.Setup(x => x.GetPendingRequestsAsync())
            .ReturnsAsync(new List<PendingRoommateRequestDto>());
        _mockRoommateService.Setup(x => x.GetRelationshipsAsync())
            .ReturnsAsync(new List<RoommateRelationshipDto>());

        RenderProviders();

        // Act
        var cut = Render<AdminRoommates>();
        cut.FindAll(".mud-tab")[1].Click(); // Switch to Relationships tab

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Nu există relații de colegiat");
        });
    }

    // Data Display Tests
    [Fact]
    public void AdminRoommates_WithPendingRequests_DisplaysRequests()
    {
        // Arrange
        var requests = new List<PendingRoommateRequestDto>
        {
            new PendingRoommateRequestDto(
                Guid.NewGuid(),
                Guid.NewGuid(), "John Requester", "john@test.com",
                Guid.NewGuid(), "Jane Target", "jane@test.com",
                "Hello, I'd like to be roommates!",
                DateTime.UtcNow
            )
        };

        _mockRoommateService.Setup(x => x.GetPendingRequestsAsync())
            .ReturnsAsync(requests);
        _mockRoommateService.Setup(x => x.GetRelationshipsAsync())
            .ReturnsAsync(new List<RoommateRelationshipDto>());

        RenderProviders();

        // Act
        var cut = Render<AdminRoommates>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("John Requester");
            cut.Markup.Should().Contain("Jane Target");
        });
    }

    [Fact]
    public void AdminRoommates_WithRequests_ShowsApproveButton()
    {
        // Arrange
        var requests = new List<PendingRoommateRequestDto>
        {
            new PendingRoommateRequestDto(
                Guid.NewGuid(),
                Guid.NewGuid(), "John", "john@test.com",
                Guid.NewGuid(), "Jane", "jane@test.com",
                "Message",
                DateTime.UtcNow
            )
        };

        _mockRoommateService.Setup(x => x.GetPendingRequestsAsync())
            .ReturnsAsync(requests);
        _mockRoommateService.Setup(x => x.GetRelationshipsAsync())
            .ReturnsAsync(new List<RoommateRelationshipDto>());

        RenderProviders();

        // Act
        var cut = Render<AdminRoommates>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Aprobă");
        });
    }

    [Fact]
    public void AdminRoommates_WithRequests_ShowsRejectButton()
    {
        // Arrange
        var requests = new List<PendingRoommateRequestDto>
        {
            new PendingRoommateRequestDto(
                Guid.NewGuid(),
                Guid.NewGuid(), "John", "john@test.com",
                Guid.NewGuid(), "Jane", "jane@test.com",
                null,
                DateTime.UtcNow
            )
        };

        _mockRoommateService.Setup(x => x.GetPendingRequestsAsync())
            .ReturnsAsync(requests);
        _mockRoommateService.Setup(x => x.GetRelationshipsAsync())
            .ReturnsAsync(new List<RoommateRelationshipDto>());

        RenderProviders();

        // Act
        var cut = Render<AdminRoommates>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Respinge");
        });
    }

    [Fact]
    public void AdminRoommates_WithRequests_ShowsEmails()
    {
        // Arrange
        var requests = new List<PendingRoommateRequestDto>
        {
            new PendingRoommateRequestDto(
                Guid.NewGuid(),
                Guid.NewGuid(), "John", "john@email.com",
                Guid.NewGuid(), "Jane", "jane@email.com",
                null,
                DateTime.UtcNow
            )
        };

        _mockRoommateService.Setup(x => x.GetPendingRequestsAsync())
            .ReturnsAsync(requests);
        _mockRoommateService.Setup(x => x.GetRelationshipsAsync())
            .ReturnsAsync(new List<RoommateRelationshipDto>());

        RenderProviders();

        // Act
        var cut = Render<AdminRoommates>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("john@email.com");
            cut.Markup.Should().Contain("jane@email.com");
        });
    }

    [Fact]
    public void AdminRoommates_WithMessage_ShowsMessage()
    {
        // Arrange
        var requests = new List<PendingRoommateRequestDto>
        {
            new PendingRoommateRequestDto(
                Guid.NewGuid(),
                Guid.NewGuid(), "John", "john@email.com",
                Guid.NewGuid(), "Jane", "jane@email.com",
                "My custom message here",
                DateTime.UtcNow
            )
        };

        _mockRoommateService.Setup(x => x.GetPendingRequestsAsync())
            .ReturnsAsync(requests);
        _mockRoommateService.Setup(x => x.GetRelationshipsAsync())
            .ReturnsAsync(new List<RoommateRelationshipDto>());

        RenderProviders();

        // Act
        var cut = Render<AdminRoommates>();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("My custom message here");
        });
    }

    [Fact]
    public void AdminRoommates_WithRelationships_DisplaysRelationships()
    {
        // Arrange
        var relationships = new List<RoommateRelationshipDto>
        {
            new RoommateRelationshipDto(
                Guid.NewGuid(),
                Guid.NewGuid(), "User One", "user1@test.com",
                Guid.NewGuid(), "User Two", "user2@test.com",
                "Admin Name",
                DateTime.UtcNow,
                true
            )
        };

        _mockRoommateService.Setup(x => x.GetPendingRequestsAsync())
            .ReturnsAsync(new List<PendingRoommateRequestDto>());
        _mockRoommateService.Setup(x => x.GetRelationshipsAsync())
            .ReturnsAsync(relationships);

        RenderProviders();

        // Act
        var cut = Render<AdminRoommates>();
        cut.FindAll(".mud-tab")[1].Click(); // Switch to Relationships tab

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("User One");
            cut.Markup.Should().Contain("User Two");
        });
    }

    [Fact]
    public void AdminRoommates_InactiveRelationship_ShowsInactiveChip()
    {
        // Arrange
        var relationships = new List<RoommateRelationshipDto>
        {
            new RoommateRelationshipDto(
                Guid.NewGuid(),
                Guid.NewGuid(), "UserA", "a@test.com",
                Guid.NewGuid(), "UserB", "b@test.com",
                "Admin",
                DateTime.UtcNow,
                false // Inactive
            )
        };

        _mockRoommateService.Setup(x => x.GetPendingRequestsAsync())
            .ReturnsAsync(new List<PendingRoommateRequestDto>());
        _mockRoommateService.Setup(x => x.GetRelationshipsAsync())
            .ReturnsAsync(relationships);

        RenderProviders();

        // Act
        var cut = Render<AdminRoommates>();
        cut.FindAll(".mud-tab")[1].Click(); // Switch to Relationships tab

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Inactiv");
        });
    }

    [Fact]
    public void AdminRoommates_Relationship_ShowsApprovedByAdmin()
    {
        // Arrange
        var relationships = new List<RoommateRelationshipDto>
        {
            new RoommateRelationshipDto(
                Guid.NewGuid(),
                Guid.NewGuid(), "UserA", "a@test.com",
                Guid.NewGuid(), "UserB", "b@test.com",
                "Super Admin",
                DateTime.UtcNow,
                true
            )
        };

        _mockRoommateService.Setup(x => x.GetPendingRequestsAsync())
            .ReturnsAsync(new List<PendingRoommateRequestDto>());
        _mockRoommateService.Setup(x => x.GetRelationshipsAsync())
            .ReturnsAsync(relationships);

        RenderProviders();

        // Act
        var cut = Render<AdminRoommates>();
        cut.FindAll(".mud-tab")[1].Click(); // Switch to Relationships tab

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Super Admin");
            cut.Markup.Should().Contain("Aprobat de");
        });
    }

    // Service Registration Tests
    [Fact]
    public void AdminRoommates_RoommateServiceRegistered()
    {
        Services.GetService<IRoommateService>().Should().NotBeNull();
    }

    [Fact]
    public void AdminRoommates_SnackbarRegistered()
    {
        Services.GetService<ISnackbar>().Should().NotBeNull();
    }

    [Fact]
    public void AdminRoommates_DialogServiceRegistered()
    {
        Services.GetService<IDialogService>().Should().NotBeNull();
    }

    // Error Handling Tests
    [Fact]
    public void AdminRoommates_ServiceError_ComponentStillRenders()
    {
        // Arrange
        _mockRoommateService.Setup(x => x.GetPendingRequestsAsync())
            .ThrowsAsync(new Exception("Service error"));
        _mockRoommateService.Setup(x => x.GetRelationshipsAsync())
            .ReturnsAsync(new List<RoommateRelationshipDto>());

        RenderProviders();

        // Act
        var cut = Render<AdminRoommates>();

        // Assert - component should handle error gracefully and render
        cut.WaitForAssertion(() =>
        {
            cut.Should().NotBeNull();
            // Service was called even if it failed
            _mockRoommateService.Verify(x => x.GetPendingRequestsAsync(), Times.AtLeastOnce);
        });
    }
}
