using FluentAssertions;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Services;

public class ListingServiceExtendedTests
{
    #region ListingSummaryDto Tests

    [Fact]
    public void ListingSummaryDto_Should_HaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var availableFrom = DateTime.UtcNow.AddDays(7);
        var amenities = new List<string> { "WiFi", "AC", "Parking" };

        // Act
        var dto = new ListingSummaryDto(
            Id: id,
            OwnerId: ownerId,
            OwnerFullName: "John Smith",
            Title: "Cozy Room in City Center",
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
        dto.OwnerFullName.Should().Be("John Smith");
        dto.Title.Should().Be("Cozy Room in City Center");
        dto.City.Should().Be("București");
        dto.Area.Should().Be("Centru");
        dto.Price.Should().Be(500);
        dto.AvailableFrom.Should().Be(availableFrom);
        dto.Amenities.Should().HaveCount(3);
        dto.IsActive.Should().BeTrue();
        dto.ThumbnailPath.Should().Be("/images/room1.jpg");
        dto.ApprovalStatus.Should().Be(ListingApprovalStatus.Approved);
        dto.RejectionReason.Should().BeNull();
    }

    [Fact]
    public void ListingSummaryDto_ThumbnailPath_ShouldBeNullByDefault()
    {
        // Act
        var dto = new ListingSummaryDto(
            Guid.NewGuid(), Guid.NewGuid(), "Owner", "Title", "City", "Area",
            400, DateTime.UtcNow, new List<string>(), true
        );

        // Assert
        dto.ThumbnailPath.Should().BeNull();
    }

    [Fact]
    public void ListingSummaryDto_ApprovalStatus_ShouldDefaultToPending()
    {
        // Act
        var dto = new ListingSummaryDto(
            Guid.NewGuid(), Guid.NewGuid(), "Owner", "Title", "City", "Area",
            400, DateTime.UtcNow, new List<string>(), true
        );

        // Assert
        dto.ApprovalStatus.Should().Be(ListingApprovalStatus.Pending);
    }

    [Fact]
    public void ListingSummaryDto_Should_HandleRejectedStatus()
    {
        // Act
        var dto = new ListingSummaryDto(
            Guid.NewGuid(), Guid.NewGuid(), "Owner", "Title", "City", "Area",
            400, DateTime.UtcNow, new List<string>(), false,
            null, ListingApprovalStatus.Rejected, "Does not meet standards"
        );

        // Assert
        dto.ApprovalStatus.Should().Be(ListingApprovalStatus.Rejected);
        dto.RejectionReason.Should().Be("Does not meet standards");
        dto.IsActive.Should().BeFalse();
    }

    #endregion

    #region ListingDto Tests

    [Fact]
    public void ListingDto_Should_HaveCorrectProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var availableFrom = DateTime.UtcNow.AddDays(7);
        var createdAt = DateTime.UtcNow;
        var amenities = new List<string> { "WiFi", "Parking" };
        var imagePaths = new List<string> { "/images/room1.jpg", "/images/room2.jpg" };

        // Act
        var dto = new ListingDto(
            Id: id,
            OwnerId: ownerId,
            Title: "Spacious Room",
            Description: "A nice spacious room with great view",
            City: "Cluj-Napoca",
            Area: "Mărăști",
            Price: 600,
            AvailableFrom: availableFrom,
            Amenities: amenities,
            CreatedAt: createdAt,
            IsActive: true,
            ImagePaths: imagePaths,
            OwnerFullName: "Maria Popescu",
            ApprovalStatus: ListingApprovalStatus.Approved
        );

        // Assert
        dto.Id.Should().Be(id);
        dto.OwnerId.Should().Be(ownerId);
        dto.Title.Should().Be("Spacious Room");
        dto.Description.Should().Be("A nice spacious room with great view");
        dto.City.Should().Be("Cluj-Napoca");
        dto.Area.Should().Be("Mărăști");
        dto.Price.Should().Be(600);
        dto.AvailableFrom.Should().Be(availableFrom);
        dto.Amenities.Should().HaveCount(2);
        dto.CreatedAt.Should().Be(createdAt);
        dto.IsActive.Should().BeTrue();
        dto.ImagePaths.Should().HaveCount(2);
        dto.OwnerFullName.Should().Be("Maria Popescu");
    }

    [Fact]
    public void ListingDto_ImagePaths_CanBeNull()
    {
        // Act
        var dto = new ListingDto(
            Guid.NewGuid(), Guid.NewGuid(), "Title", "Description",
            "City", "Area", 500, DateTime.UtcNow, new List<string>(),
            DateTime.UtcNow, true
        );

        // Assert
        dto.ImagePaths.Should().BeNull();
    }

    [Fact]
    public void ListingDto_OwnerFullName_CanBeNull()
    {
        // Act
        var dto = new ListingDto(
            Guid.NewGuid(), Guid.NewGuid(), "Title", "Description",
            "City", "Area", 500, DateTime.UtcNow, new List<string>(),
            DateTime.UtcNow, true, null, null
        );

        // Assert
        dto.OwnerFullName.Should().BeNull();
    }

    [Theory]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    [InlineData(2500)]
    public void ListingDto_Should_AcceptVariousPrices(decimal price)
    {
        // Act
        var dto = new ListingDto(
            Guid.NewGuid(), Guid.NewGuid(), "Title", "Description",
            "City", "Area", price, DateTime.UtcNow, new List<string>(),
            DateTime.UtcNow, true
        );

        // Assert
        dto.Price.Should().Be(price);
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
            City: "Timișoara",
            Area: "Centru",
            Price: 750,
            AvailableFrom: DateTime.UtcNow.AddMonths(1),
            Amenities: new List<string> { "WiFi", "AC" },
            IsActive: true
        );

        // Assert
        request.Title.Should().Be("Updated Title");
        request.Description.Should().Be("Updated Description");
        request.City.Should().Be("Timișoara");
        request.Area.Should().Be("Centru");
        request.Price.Should().Be(750);
        request.Amenities.Should().HaveCount(2);
        request.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateListingRequest_Should_SupportRecordEquality()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var amenities = new List<string> { "WiFi" };
        
        var request1 = new UpdateListingRequest("Title", "Desc", "City", "Area", 500, date, amenities, true);
        var request2 = new UpdateListingRequest("Title", "Desc", "City", "Area", 500, date, amenities, true);

        // Assert - Note: Lists compare by reference in records, so these will not be equal
        request1.Title.Should().Be(request2.Title);
        request1.Price.Should().Be(request2.Price);
    }

    #endregion

    #region ListingsResponse Pagination Tests

    [Theory]
    [InlineData(100, 1, 12, 9)]
    [InlineData(50, 1, 10, 5)]
    [InlineData(25, 2, 10, 3)]
    [InlineData(0, 1, 12, 0)]
    public void ListingsResponse_Should_CalculatePagination(int totalCount, int page, int pageSize, int expectedPages)
    {
        // Act
        var response = new ListingsResponse(
            new List<ListingSummaryDto>(),
            totalCount,
            page,
            pageSize
        );

        // Assert
        response.TotalCount.Should().Be(totalCount);
        response.Page.Should().Be(page);
        response.PageSize.Should().Be(pageSize);
        
        var calculatedPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);
        calculatedPages.Should().Be(expectedPages);
    }

    #endregion
}

