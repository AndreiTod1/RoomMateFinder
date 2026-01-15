using FluentAssertions;
using RoomMate_Finder.Entities;
using Xunit;

namespace RoomMate_Finder.Test.Entities;

public class RoomListingImageTests
{
    [Fact]
    public void Given_NewRoomListingImage_When_Created_Then_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var image = new RoomListingImage();

        // Assert
        image.Id.Should().Be(Guid.Empty);
        image.RoomListingId.Should().Be(Guid.Empty);
        image.ImagePath.Should().Be(string.Empty);
        image.DisplayOrder.Should().Be(0);
        image.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Given_RoomListingImage_When_PropertiesSet_Then_ShouldReturnCorrectValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var listingId = Guid.NewGuid();
        var imagePath = "/uploads/images/room1.jpg";
        var displayOrder = 3;
        var createdAt = DateTime.UtcNow.AddDays(-1);

        // Act
        var image = new RoomListingImage
        {
            Id = id,
            RoomListingId = listingId,
            ImagePath = imagePath,
            DisplayOrder = displayOrder,
            CreatedAt = createdAt
        };

        // Assert
        image.Id.Should().Be(id);
        image.RoomListingId.Should().Be(listingId);
        image.ImagePath.Should().Be(imagePath);
        image.DisplayOrder.Should().Be(displayOrder);
        image.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void Given_RoomListingImage_When_RoomListingAssigned_Then_ShouldBeAccessible()
    {
        // Arrange
        var roomListing = new RoomListing
        {
            Id = Guid.NewGuid(),
            Title = "Test Listing",
            OwnerId = Guid.NewGuid()
        };

        // Act
        var image = new RoomListingImage
        {
            Id = Guid.NewGuid(),
            RoomListingId = roomListing.Id,
            RoomListing = roomListing,
            ImagePath = "/uploads/test.jpg"
        };

        // Assert
        image.RoomListing.Should().NotBeNull();
        image.RoomListing.Title.Should().Be("Test Listing");
        image.RoomListingId.Should().Be(roomListing.Id);
    }

    [Fact]
    public void Given_MultipleImages_When_DisplayOrderSet_Then_ShouldMaintainOrder()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        var images = new List<RoomListingImage>
        {
            new() { Id = Guid.NewGuid(), RoomListingId = listingId, ImagePath = "/img1.jpg", DisplayOrder = 2 },
            new() { Id = Guid.NewGuid(), RoomListingId = listingId, ImagePath = "/img2.jpg", DisplayOrder = 1 },
            new() { Id = Guid.NewGuid(), RoomListingId = listingId, ImagePath = "/img3.jpg", DisplayOrder = 3 }
        };

        // Act
        var orderedImages = images.OrderBy(i => i.DisplayOrder).ToList();

        // Assert
        orderedImages[0].ImagePath.Should().Be("/img2.jpg");
        orderedImages[1].ImagePath.Should().Be("/img1.jpg");
        orderedImages[2].ImagePath.Should().Be("/img3.jpg");
    }
}
