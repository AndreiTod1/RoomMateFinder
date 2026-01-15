using FluentAssertions;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.RoomListings.DeleteListing;
using RoomMate_Finder.Test.Helpers;

namespace RoomMate_Finder.Test.Handlers;

public class DeleteListingHandlerTests
{
    private static Profile CreateTestProfile(string role = "User")
    {
        return new Profile
        {
            Id = Guid.NewGuid(),
            Email = $"test{Guid.NewGuid():N}@test.com",
            PasswordHash = "hashed",
            FullName = "Test User",
            Age = 25,
            Gender = "Male",
            University = "Test Uni",
            Bio = "Bio",
            Lifestyle = "quiet",
            Interests = "music",
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Given_NonExistentListing_When_HandleIsCalled_Then_ReturnsFailure()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var handler = new DeleteListingHandler(context);
        var command = new DeleteListingCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task Given_UnauthorizedUser_When_HandleIsCalled_Then_ReturnsFailure()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var owner = CreateTestProfile();
        var otherUser = CreateTestProfile();
        context.Profiles.AddRange(owner, otherUser);
        
        var listing = new RoomListing
        {
            Id = Guid.NewGuid(),
            OwnerId = owner.Id,
            Title = "Test",
            Description = "Test",
            City = "Test",
            Area = "Test",
            Price = 100,
            AvailableFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.RoomListings.Add(listing);
        await context.SaveChangesAsync();

        var handler = new DeleteListingHandler(context);
        var command = new DeleteListingCommand(listing.Id, otherUser.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not authorized");
    }

    [Fact]
    public async Task Given_Owner_When_HandleIsCalled_Then_ListingIsDeleted()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var owner = CreateTestProfile();
        context.Profiles.Add(owner);
        
        var listing = new RoomListing
        {
            Id = Guid.NewGuid(),
            OwnerId = owner.Id,
            Title = "Owner Listing",
            Description = "Test",
            City = "Test",
            Area = "Test",
            Price = 100,
            AvailableFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.RoomListings.Add(listing);
        await context.SaveChangesAsync();

        var handler = new DeleteListingHandler(context);
        var command = new DeleteListingCommand(listing.Id, owner.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("deleted successfully");
        
        var deletedListing = await context.RoomListings.FindAsync(listing.Id);
        deletedListing.Should().BeNull();
    }

    [Fact]
    public async Task Given_Admin_When_HandleIsCalled_Then_ListingIsDeleted()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var owner = CreateTestProfile("User");
        var admin = CreateTestProfile("Admin");
        context.Profiles.AddRange(owner, admin);
        
        var listing = new RoomListing
        {
            Id = Guid.NewGuid(),
            OwnerId = owner.Id,
            Title = "Admin Deletion",
            Description = "Test",
            City = "Test",
            Area = "Test",
            Price = 100,
            AvailableFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.RoomListings.Add(listing);
        await context.SaveChangesAsync();

        var handler = new DeleteListingHandler(context);
        var command = new DeleteListingCommand(listing.Id, admin.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        
        var deletedListing = await context.RoomListings.FindAsync(listing.Id);
        deletedListing.Should().BeNull();
    }
}
