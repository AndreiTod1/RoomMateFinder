using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.RoomListings.ApproveRejectListing;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Test.Handlers;

public class AdminListingsHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ApproveListingHandler _approveHandler;
    private readonly RejectListingHandler _rejectHandler;

    public AdminListingsHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _approveHandler = new ApproveListingHandler(_context);
        _rejectHandler = new RejectListingHandler(_context);
    }

    [Fact]
    public async Task Approve_GivenPendingListing_ShouldApproveListing()
    {
        // Arrange
        var listing = new RoomListing
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Title = "L1",
            City = "C",
            Area = "A",
            Price = 100,
            Amenities = "",
            AvailableFrom = DateTime.Today,
            IsActive = true,
            ApprovalStatus = ListingApprovalStatus.Pending
        };
        _context.RoomListings.Add(listing);
        await _context.SaveChangesAsync();

        var adminId = Guid.NewGuid();
        var command = new ApproveListingCommand(listing.Id, adminId);

        // Act
        var result = await _approveHandler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("approved");

        var dbListing = await _context.RoomListings.FindAsync(listing.Id);
        dbListing!.ApprovalStatus.Should().Be(ListingApprovalStatus.Approved);
        dbListing.ApprovedByAdminId.Should().Be(adminId);
        dbListing.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Approve_GivenAlreadyApprovedListing_ShouldReturnFalse()
    {
        // Arrange
        var listing = new RoomListing
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Title = "L2",
            City = "C",
            Area = "A",
            Price = 100,
            Amenities = "",
            AvailableFrom = DateTime.Today,
            IsActive = true,
            ApprovalStatus = ListingApprovalStatus.Approved
        };
        _context.RoomListings.Add(listing);
        await _context.SaveChangesAsync();

        var command = new ApproveListingCommand(listing.Id, Guid.NewGuid());

        // Act
        var result = await _approveHandler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already approved");
    }

    [Fact]
    public async Task Reject_GivenPendingListing_ShouldRejectListing()
    {
        // Arrange
        var listing = new RoomListing
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Title = "L3",
            City = "C",
            Area = "A",
            Price = 100,
            Amenities = "",
            AvailableFrom = DateTime.Today,
            IsActive = true,
            ApprovalStatus = ListingApprovalStatus.Pending
        };
        _context.RoomListings.Add(listing);
        await _context.SaveChangesAsync();

        var adminId = Guid.NewGuid();
        var reason = "Inappropriate content";
        var command = new RejectListingCommand(listing.Id, adminId, reason);

        // Act
        var result = await _rejectHandler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("rejected");

        var dbListing = await _context.RoomListings.FindAsync(listing.Id);
        dbListing!.ApprovalStatus.Should().Be(ListingApprovalStatus.Rejected);
        dbListing.ApprovedByAdminId.Should().Be(adminId);
        dbListing.RejectionReason.Should().Be(reason);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
