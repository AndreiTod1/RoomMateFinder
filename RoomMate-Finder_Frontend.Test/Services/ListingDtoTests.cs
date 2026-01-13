using FluentAssertions;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Services;

public class ListingDtoTests
{
    #region ListingApprovalStatus Tests

    [Fact]
    public void ListingApprovalStatus_Pending_ShouldBe0()
    {
        ((int)ListingApprovalStatus.Pending).Should().Be(0);
    }

    [Fact]
    public void ListingApprovalStatus_Approved_ShouldBe1()
    {
        ((int)ListingApprovalStatus.Approved).Should().Be(1);
    }

    [Fact]
    public void ListingApprovalStatus_Rejected_ShouldBe2()
    {
        ((int)ListingApprovalStatus.Rejected).Should().Be(2);
    }

    #endregion

    #region ListingsSearchRequest Tests

    [Fact]
    public void ListingsSearchRequest_Should_HaveDefaultValues()
    {
        // Act
        var request = new ListingsSearchRequest();

        // Assert
        request.City.Should().BeNull();
        request.Area.Should().BeNull();
        request.MinPrice.Should().BeNull();
        request.MaxPrice.Should().BeNull();
        request.OwnerId.Should().BeNull();
        request.IncludeInactive.Should().BeFalse();
        request.ApprovalStatus.Should().BeNull();
        request.IncludePending.Should().BeFalse();
        request.Page.Should().Be(1);
        request.PageSize.Should().Be(12);
    }

    [Fact]
    public void ListingsSearchRequest_Should_AllowCustomValues()
    {
        // Arrange
        var ownerId = Guid.NewGuid();

        // Act
        var request = new ListingsSearchRequest(
            City: "București",
            Area: "Centru",
            MinPrice: 300,
            MaxPrice: 800,
            OwnerId: ownerId,
            IncludeInactive: true,
            ApprovalStatus: ListingApprovalStatus.Approved,
            IncludePending: true,
            Page: 3,
            PageSize: 20
        );

        // Assert
        request.City.Should().Be("București");
        request.Area.Should().Be("Centru");
        request.MinPrice.Should().Be(300);
        request.MaxPrice.Should().Be(800);
        request.OwnerId.Should().Be(ownerId);
        request.IncludeInactive.Should().BeTrue();
        request.ApprovalStatus.Should().Be(ListingApprovalStatus.Approved);
        request.IncludePending.Should().BeTrue();
        request.Page.Should().Be(3);
        request.PageSize.Should().Be(20);
    }

    #endregion

    #region ListingsResponse Tests

    [Fact]
    public void ListingsResponse_Should_HaveCorrectProperties()
    {
        // Arrange
        var listings = new List<ListingSummaryDto>
        {
            new ListingSummaryDto(Guid.NewGuid(), Guid.NewGuid(), "Owner 1", "Room 1", "București", "Centru", 500, DateTime.UtcNow.AddDays(7), new List<string> { "WiFi" }, true),
            new ListingSummaryDto(Guid.NewGuid(), Guid.NewGuid(), "Owner 2", "Room 2", "Cluj", "Centru", 600, DateTime.UtcNow.AddDays(14), new List<string> { "AC", "Parking" }, true)
        };

        // Act
        var response = new ListingsResponse(
            Listings: listings,
            TotalCount: 50,
            Page: 2,
            PageSize: 12
        );

        // Assert
        response.Listings.Should().HaveCount(2);
        response.TotalCount.Should().Be(50);
        response.Page.Should().Be(2);
        response.PageSize.Should().Be(12);
    }

    [Fact]
    public void ListingsResponse_Should_HandleEmptyList()
    {
        // Act
        var response = new ListingsResponse(
            Listings: new List<ListingSummaryDto>(),
            TotalCount: 0,
            Page: 1,
            PageSize: 12
        );

        // Assert
        response.Listings.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
    }

    #endregion

    #region ListingSummaryDto Tests

    [Fact]
    public void ListingSummaryDto_Should_HaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var availableFrom = DateTime.UtcNow.AddDays(30);
        var amenities = new List<string> { "WiFi", "AC", "Parking", "Balcony" };

        // Act
        var dto = new ListingSummaryDto(
            Id: id,
            OwnerId: ownerId,
            OwnerFullName: "John Doe",
            Title: "Beautiful Room in Center",
            City: "București",
            Area: "Centru",
            Price: 500,
            AvailableFrom: availableFrom,
            Amenities: amenities,
            IsActive: true,
            ThumbnailPath: "/images/room1.jpg",
            ApprovalStatus: ListingApprovalStatus.Approved,
            RejectionReason: null
        );

        // Assert
        dto.Id.Should().Be(id);
        dto.OwnerId.Should().Be(ownerId);
        dto.OwnerFullName.Should().Be("John Doe");
        dto.Title.Should().Be("Beautiful Room in Center");
        dto.City.Should().Be("București");
        dto.Area.Should().Be("Centru");
        dto.Price.Should().Be(500);
        dto.AvailableFrom.Should().Be(availableFrom);
        dto.Amenities.Should().HaveCount(4);
        dto.IsActive.Should().BeTrue();
        dto.ThumbnailPath.Should().Be("/images/room1.jpg");
        dto.ApprovalStatus.Should().Be(ListingApprovalStatus.Approved);
        dto.RejectionReason.Should().BeNull();
    }

    [Fact]
    public void ListingSummaryDto_ThumbnailPath_CanBeNull()
    {
        // Act
        var dto = new ListingSummaryDto(
            Guid.NewGuid(), Guid.NewGuid(), "Owner", "Title", "City", "Area", 400, DateTime.UtcNow, new List<string>(), true
        );

        // Assert
        dto.ThumbnailPath.Should().BeNull();
    }

    [Fact]
    public void ListingSummaryDto_Should_ShowRejectionReason_WhenRejected()
    {
        // Act
        var dto = new ListingSummaryDto(
            Id: Guid.NewGuid(),
            OwnerId: Guid.NewGuid(),
            OwnerFullName: "Owner",
            Title: "Rejected Listing",
            City: "City",
            Area: "Area",
            Price: 300,
            AvailableFrom: DateTime.UtcNow,
            Amenities: new List<string>(),
            IsActive: false,
            ThumbnailPath: null,
            ApprovalStatus: ListingApprovalStatus.Rejected,
            RejectionReason: "Inappropriate content"
        );

        // Assert
        dto.ApprovalStatus.Should().Be(ListingApprovalStatus.Rejected);
        dto.RejectionReason.Should().Be("Inappropriate content");
    }

    #endregion

    #region ListingDto Tests

    [Fact]
    public void ListingDto_Should_HaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var availableFrom = DateTime.UtcNow.AddDays(30);
        var imagePaths = new List<string> { "/images/1.jpg", "/images/2.jpg" };

        // Act
        var dto = new ListingDto(
            Id: id,
            OwnerId: ownerId,
            Title: "Spacious Room",
            Description: "A spacious room with great view",
            City: "Cluj",
            Area: "Mărăști",
            Price: 600,
            AvailableFrom: availableFrom,
            Amenities: new List<string> { "WiFi", "AC" },
            CreatedAt: createdAt,
            IsActive: true,
            ImagePaths: imagePaths,
            OwnerFullName: "Jane Doe",
            ApprovalStatus: ListingApprovalStatus.Approved,
            RejectionReason: null
        );

        // Assert
        dto.Id.Should().Be(id);
        dto.OwnerId.Should().Be(ownerId);
        dto.Title.Should().Be("Spacious Room");
        dto.Description.Should().Be("A spacious room with great view");
        dto.City.Should().Be("Cluj");
        dto.Area.Should().Be("Mărăști");
        dto.Price.Should().Be(600);
        dto.AvailableFrom.Should().Be(availableFrom);
        dto.Amenities.Should().HaveCount(2);
        dto.CreatedAt.Should().Be(createdAt);
        dto.IsActive.Should().BeTrue();
        dto.ImagePaths.Should().HaveCount(2);
        dto.OwnerFullName.Should().Be("Jane Doe");
    }

    [Fact]
    public void ListingDto_ImagePaths_CanBeNull()
    {
        // Act
        var dto = new ListingDto(
            Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", "City", "Area", 500, DateTime.UtcNow, new List<string>(), DateTime.UtcNow, true
        );

        // Assert
        dto.ImagePaths.Should().BeNull();
    }

    #endregion

    #region UpdateListingRequest Tests

    [Fact]
    public void UpdateListingRequest_Should_HaveCorrectProperties()
    {
        // Act
        var request = new UpdateListingRequest(
            Title: "Updated Title",
            Description: "Updated Description",
            City: "Updated City",
            Area: "Updated Area",
            Price: 750,
            AvailableFrom: DateTime.UtcNow.AddDays(60),
            Amenities: new List<string> { "WiFi", "AC", "Pool" },
            IsActive: false
        );

        // Assert
        request.Title.Should().Be("Updated Title");
        request.Description.Should().Be("Updated Description");
        request.City.Should().Be("Updated City");
        request.Area.Should().Be("Updated Area");
        request.Price.Should().Be(750);
        request.Amenities.Should().HaveCount(3);
        request.IsActive.Should().BeFalse();
    }

    #endregion
}

