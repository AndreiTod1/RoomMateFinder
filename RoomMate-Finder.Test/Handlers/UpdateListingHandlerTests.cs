using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.RoomListings.UpdateListing;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Test.Handlers;

public class UpdateListingHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UpdateListingHandler _handler;

    public UpdateListingHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _handler = new UpdateListingHandler(_context);
    }

    [Fact]
    public async Task Handle_GivenValidRequest_WhenListingExistsAndBelongsToOwner_ShouldUpdateListing()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var listing = new RoomListing
        {
            Id = listingId,
            OwnerId = ownerId,
            Title = "Old Title",
            Description = "Old Desc",
            City = "Old City",
            Area = "Old Area",
            Price = 100,
            AvailableFrom = DateTime.UtcNow,
            Amenities = "A,B",
            IsActive = true
        };
        _context.RoomListings.Add(listing);
        await _context.SaveChangesAsync();

        var request = new UpdateListingRequest
        {
            Id = listingId,
            OwnerId = ownerId,
            Title = "New Title",
            Description = "New Desc",
            City = "New City",
            Area = "New Area",
            Price = 200,
            AvailableFrom = DateTime.UtcNow.AddDays(1),
            Amenities = new List<string> { "C", "D" },
            IsActive = false
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("New Title");
        result.Price.Should().Be(200);
        result.IsActive.Should().BeFalse();

        var updatedListing = await _context.RoomListings.FindAsync(listingId);
        updatedListing.Should().NotBeNull();
        updatedListing!.Title.Should().Be("New Title");
        updatedListing.Amenities.Should().Contain("C");
    }

    [Fact]
    public async Task Handle_GivenNonExistentListing_ShouldReturnNull()
    {
        // Arrange
        var request = new UpdateListingRequest
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Title = "Title"
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_GivenListingBelongsToDifferentOwner_ShouldReturnNull()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherOwnerId = Guid.NewGuid();

        var listing = new RoomListing
        {
            Id = listingId,
            OwnerId = ownerId,
            Title = "Title",
            Description = "Desc",
            City = "City",
            Area = "Area",
            Price = 100,
            AvailableFrom = DateTime.UtcNow,
            Amenities = "A"
        };
        _context.RoomListings.Add(listing);
        await _context.SaveChangesAsync();

        var request = new UpdateListingRequest
        {
            Id = listingId,
            OwnerId = otherOwnerId, // Different owner
            Title = "New Title"
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeNull(); // Treated as not found for security
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
