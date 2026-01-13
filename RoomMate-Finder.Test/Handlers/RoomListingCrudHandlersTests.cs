using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.RoomListings.ApproveRejectListing;
using RoomMate_Finder.Features.RoomListings.CreateListing;
using RoomMate_Finder.Features.RoomListings.UpdateListing;
using RoomMate_Finder.Test.Helpers;

namespace RoomMate_Finder.Test.Handlers;

public class RoomListingCrudHandlersTests : IDisposable
{
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly string _tempPath;

    public RoomListingCrudHandlersTests()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), "RoomListingTest_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempPath);
        Directory.CreateDirectory(Path.Combine(_tempPath, "room-images"));
        
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(e => e.WebRootPath).Returns(_tempPath);
    }

    #region CreateListingHandler Tests

    [Fact]
    public async Task Given_NonExistentOwner_When_CreateListingHandlerIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var handler = new CreateListingHandler(context, _mockEnvironment.Object);
        
        var request = new CreateListingRequest
        {
            OwnerId = Guid.NewGuid(),
            Title = "Test Listing",
            Description = "Test Description",
            City = "Bucharest",
            Area = "Center",
            Price = 500,
            AvailableFrom = DateTime.UtcNow.AddDays(7),
            Amenities = new List<string> { "WiFi", "AC" }
        };
        var command = new CreateListingWithImagesCommand(request, null, false);

        // Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Owner profile not found*");
    }

    [Fact]
    public async Task Given_ValidRequest_When_CreateListingHandlerIsCalled_Then_ListingIsCreatedWithPendingStatus()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var owner = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "owner@test.com",
            PasswordHash = "hash",
            FullName = "Test Owner",
            Age = 25,
            Gender = "M",
            CreatedAt = DateTime.UtcNow
        };
        context.Profiles.Add(owner);
        await context.SaveChangesAsync();

        var handler = new CreateListingHandler(context, _mockEnvironment.Object);
        var request = new CreateListingRequest
        {
            OwnerId = owner.Id,
            Title = "Beautiful Room",
            Description = "A beautiful room in the center",
            City = "Bucharest",
            Area = "Center",
            Price = 500,
            AvailableFrom = DateTime.UtcNow.AddDays(7),
            Amenities = new List<string> { "WiFi", "AC", "Parking" }
        };
        var command = new CreateListingWithImagesCommand(request, null, false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Title.Should().Be("Beautiful Room");
        result.City.Should().Be("Bucharest");
        result.ApprovalStatus.Should().Be(ListingApprovalStatus.Pending);
    }

    [Fact]
    public async Task Given_AdminCreatesListing_When_CreateListingHandlerIsCalled_Then_ListingIsAutoApproved()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var admin = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "admin@test.com",
            PasswordHash = "hash",
            FullName = "Admin User",
            Age = 30,
            Gender = "M",
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        };
        context.Profiles.Add(admin);
        await context.SaveChangesAsync();

        var handler = new CreateListingHandler(context, _mockEnvironment.Object);
        var request = new CreateListingRequest
        {
            OwnerId = admin.Id,
            Title = "Admin Listing",
            Description = "Admin created listing",
            City = "Cluj",
            Area = "Center",
            Price = 600,
            AvailableFrom = DateTime.UtcNow.AddDays(1),
            Amenities = new List<string> { "WiFi" }
        };
        var command = new CreateListingWithImagesCommand(request, null, true); // isAdmin = true

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ApprovalStatus.Should().Be(ListingApprovalStatus.Approved);
    }

    [Fact]
    public async Task Given_TooManyImages_When_CreateListingHandlerIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var owner = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "owner@test.com",
            PasswordHash = "hash",
            FullName = "Test Owner",
            Age = 25,
            Gender = "M",
            CreatedAt = DateTime.UtcNow
        };
        context.Profiles.Add(owner);
        await context.SaveChangesAsync();

        var handler = new CreateListingHandler(context, _mockEnvironment.Object);
        var request = new CreateListingRequest
        {
            OwnerId = owner.Id,
            Title = "Test",
            Description = "Test",
            City = "Test",
            Area = "Test",
            Price = 100,
            AvailableFrom = DateTime.UtcNow,
            Amenities = new List<string>()
        };
        
        // Create mock for 9 images (more than max 8)
        var mockImages = new List<Microsoft.AspNetCore.Http.IFormFile>();
        for (int i = 0; i < 9; i++)
        {
            var mockFile = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);
            mockImages.Add(mockFile.Object);
        }
        
        var command = new CreateListingWithImagesCommand(request, mockImages, false);

        // Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Maximum 8 images*");
    }

    #endregion

    #region UpdateListingHandler Tests

    [Fact]
    public async Task Given_NonExistentListing_When_UpdateListingHandlerIsCalled_Then_ReturnsNull()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var handler = new UpdateListingHandler(context);
        
        var request = new UpdateListingRequest
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Title = "Updated Title",
            Description = "Updated Description",
            City = "Bucharest",
            Area = "Center",
            Price = 600,
            AvailableFrom = DateTime.UtcNow,
            Amenities = new List<string>(),
            IsActive = true
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_WrongOwner_When_UpdateListingHandlerIsCalled_Then_ReturnsNull()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var owner = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "owner@test.com",
            PasswordHash = "hash",
            FullName = "Owner",
            Age = 25,
            Gender = "M",
            CreatedAt = DateTime.UtcNow
        };
        var listing = new RoomListing
        {
            Id = Guid.NewGuid(),
            OwnerId = owner.Id,
            Title = "Original Title",
            Description = "Original",
            City = "Bucharest",
            Area = "Center",
            Price = 500,
            AvailableFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Profiles.Add(owner);
        context.RoomListings.Add(listing);
        await context.SaveChangesAsync();

        var handler = new UpdateListingHandler(context);
        var request = new UpdateListingRequest
        {
            Id = listing.Id,
            OwnerId = Guid.NewGuid(), // Wrong owner
            Title = "Updated",
            Description = "Updated",
            City = "Cluj",
            Area = "Center",
            Price = 600,
            AvailableFrom = DateTime.UtcNow,
            Amenities = new List<string>(),
            IsActive = true
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_ValidRequest_When_UpdateListingHandlerIsCalled_Then_ListingIsUpdated()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var owner = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "owner@test.com",
            PasswordHash = "hash",
            FullName = "Owner",
            Age = 25,
            Gender = "M",
            CreatedAt = DateTime.UtcNow
        };
        var listing = new RoomListing
        {
            Id = Guid.NewGuid(),
            OwnerId = owner.Id,
            Title = "Original Title",
            Description = "Original",
            City = "Bucharest",
            Area = "Center",
            Price = 500,
            AvailableFrom = DateTime.UtcNow,
            Amenities = "WiFi",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Profiles.Add(owner);
        context.RoomListings.Add(listing);
        await context.SaveChangesAsync();

        var handler = new UpdateListingHandler(context);
        var request = new UpdateListingRequest
        {
            Id = listing.Id,
            OwnerId = owner.Id,
            Title = "Updated Title",
            Description = "Updated Description",
            City = "Cluj",
            Area = "North",
            Price = 750,
            AvailableFrom = DateTime.UtcNow.AddDays(30),
            Amenities = new List<string> { "WiFi", "Parking", "AC" },
            IsActive = false
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Title");
        result.City.Should().Be("Cluj");
        result.Price.Should().Be(750);
        result.IsActive.Should().BeFalse();
        result.Amenities.Should().Contain("Parking");
    }

    #endregion

    #region ApproveListingHandler Tests

    [Fact]
    public async Task Given_NonExistentListing_When_ApproveListingHandlerIsCalled_Then_ReturnsFailure()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var handler = new ApproveListingHandler(context);
        var command = new ApproveListingCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task Given_AlreadyApprovedListing_When_ApproveListingHandlerIsCalled_Then_ReturnsFailure()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var listing = new RoomListing
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            City = "Test",
            Area = "Test",
            Price = 100,
            AvailableFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            ApprovalStatus = ListingApprovalStatus.Approved
        };
        context.RoomListings.Add(listing);
        await context.SaveChangesAsync();

        var handler = new ApproveListingHandler(context);
        var command = new ApproveListingCommand(listing.Id, Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already approved");
    }

    [Fact]
    public async Task Given_PendingListing_When_ApproveListingHandlerIsCalled_Then_ListingIsApproved()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var adminId = Guid.NewGuid();
        var listing = new RoomListing
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Title = "Pending Listing",
            Description = "Needs approval",
            City = "Test",
            Area = "Test",
            Price = 100,
            AvailableFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            ApprovalStatus = ListingApprovalStatus.Pending
        };
        context.RoomListings.Add(listing);
        await context.SaveChangesAsync();

        var handler = new ApproveListingHandler(context);
        var command = new ApproveListingCommand(listing.Id, adminId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("approved successfully");
        
        var updatedListing = await context.RoomListings.FindAsync(listing.Id);
        updatedListing!.ApprovalStatus.Should().Be(ListingApprovalStatus.Approved);
        updatedListing.ApprovedByAdminId.Should().Be(adminId);
    }

    #endregion

    #region RejectListingHandler Tests

    [Fact]
    public async Task Given_NonExistentListing_When_RejectListingHandlerIsCalled_Then_ReturnsFailure()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var handler = new RejectListingHandler(context);
        var command = new RejectListingCommand(Guid.NewGuid(), Guid.NewGuid(), "Not good");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task Given_AlreadyRejectedListing_When_RejectListingHandlerIsCalled_Then_ReturnsFailure()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var listing = new RoomListing
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Title = "Test",
            Description = "Test",
            City = "Test",
            Area = "Test",
            Price = 100,
            AvailableFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            ApprovalStatus = ListingApprovalStatus.Rejected
        };
        context.RoomListings.Add(listing);
        await context.SaveChangesAsync();

        var handler = new RejectListingHandler(context);
        var command = new RejectListingCommand(listing.Id, Guid.NewGuid(), "Duplicate rejection");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already rejected");
    }

    [Fact]
    public async Task Given_PendingListing_When_RejectListingHandlerIsCalled_Then_ListingIsRejected()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var adminId = Guid.NewGuid();
        var listing = new RoomListing
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Title = "Bad Listing",
            Description = "Will be rejected",
            City = "Test",
            Area = "Test",
            Price = 100,
            AvailableFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            ApprovalStatus = ListingApprovalStatus.Pending
        };
        context.RoomListings.Add(listing);
        await context.SaveChangesAsync();

        var handler = new RejectListingHandler(context);
        var command = new RejectListingCommand(listing.Id, adminId, "Incomplete information");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        
        var updatedListing = await context.RoomListings.FindAsync(listing.Id);
        updatedListing!.ApprovalStatus.Should().Be(ListingApprovalStatus.Rejected);
        updatedListing.RejectionReason.Should().Be("Incomplete information");
    }

    #endregion

    public void Dispose()
    {
        if (Directory.Exists(_tempPath))
        {
            try { Directory.Delete(_tempPath, true); } catch { /* Cleanup best effort */ }
        }
        GC.SuppressFinalize(this);
    }
}
