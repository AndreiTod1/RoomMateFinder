using FluentAssertions;
using RoomMate_Finder_Frontend.Models;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Services;

public class CollectionOperationsTests
{
    #region Listing Filtering Tests

    [Fact]
    public void Listings_FilterByCity_ShouldWork()
    {
        // Arrange
        var listings = CreateSampleListings();

        // Act
        var filtered = listings.Where(l => l.City == "București").ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.All(l => l.City == "București").Should().BeTrue();
    }

    [Fact]
    public void Listings_FilterByPriceRange_ShouldWork()
    {
        // Arrange
        var listings = CreateSampleListings();

        // Act
        var filtered = listings.Where(l => l.Price >= 400 && l.Price <= 600).ToList();

        // Assert
        filtered.Should().HaveCount(3);
        filtered.All(l => l.Price >= 400 && l.Price <= 600).Should().BeTrue();
    }

    [Fact]
    public void Listings_FilterByActiveStatus_ShouldWork()
    {
        // Arrange
        var listings = CreateSampleListings();

        // Act
        var activeListings = listings.Where(l => l.IsActive).ToList();

        // Assert
        activeListings.Should().HaveCount(4);
    }

    [Fact]
    public void Listings_FilterByApprovalStatus_ShouldWork()
    {
        // Arrange
        var listings = CreateSampleListings();

        // Act
        var approvedListings = listings.Where(l => l.ApprovalStatus == ListingApprovalStatus.Approved).ToList();

        // Assert
        approvedListings.Should().HaveCount(3);
    }

    [Fact]
    public void Listings_SortByPrice_Ascending_ShouldWork()
    {
        // Arrange
        var listings = CreateSampleListings();

        // Act
        var sorted = listings.OrderBy(l => l.Price).ToList();

        // Assert
        sorted.First().Price.Should().Be(300);
        sorted.Last().Price.Should().Be(700);
    }

    [Fact]
    public void Listings_SortByPrice_Descending_ShouldWork()
    {
        // Arrange
        var listings = CreateSampleListings();

        // Act
        var sorted = listings.OrderByDescending(l => l.Price).ToList();

        // Assert
        sorted.First().Price.Should().Be(700);
        sorted.Last().Price.Should().Be(300);
    }

    private static List<ListingSummaryDto> CreateSampleListings()
    {
        return new List<ListingSummaryDto>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Owner 1", "Room 1", "București", "Centru", 500, DateTime.UtcNow.AddDays(7), new List<string> { "WiFi" }, true, null, ListingApprovalStatus.Approved),
            new(Guid.NewGuid(), Guid.NewGuid(), "Owner 2", "Room 2", "București", "Floreasca", 600, DateTime.UtcNow.AddDays(14), new List<string> { "AC" }, true, null, ListingApprovalStatus.Approved),
            new(Guid.NewGuid(), Guid.NewGuid(), "Owner 3", "Room 3", "Cluj", "Centru", 400, DateTime.UtcNow.AddDays(7), new List<string> { "Parking" }, true, null, ListingApprovalStatus.Approved),
            new(Guid.NewGuid(), Guid.NewGuid(), "Owner 4", "Room 4", "Timișoara", "Centru", 300, DateTime.UtcNow.AddDays(21), new List<string>(), true, null, ListingApprovalStatus.Pending),
            new(Guid.NewGuid(), Guid.NewGuid(), "Owner 5", "Room 5", "Iași", "Copou", 700, DateTime.UtcNow.AddDays(10), new List<string> { "WiFi", "AC" }, false, null, ListingApprovalStatus.Rejected)
        };
    }

    #endregion

    #region Matches Filtering Tests

    [Fact]
    public void Matches_FilterByActive_ShouldWork()
    {
        // Arrange
        var matches = CreateSampleMatches();

        // Act
        var activeMatches = matches.Where(m => m.IsActive).ToList();

        // Assert
        activeMatches.Should().HaveCount(3);
    }

    [Fact]
    public void Matches_SortByMatchedAt_ShouldWork()
    {
        // Arrange
        var matches = CreateSampleMatches();

        // Act
        var sorted = matches.OrderByDescending(m => m.MatchedAt).ToList();

        // Assert - most recent first
        sorted.First().MatchedAt.Should().BeAfter(sorted.Last().MatchedAt);
    }

    [Fact]
    public void Matches_FilterByUniversity_ShouldWork()
    {
        // Arrange
        var matches = CreateSampleMatches();

        // Act
        var sameUniMatches = matches.Where(m => m.University == "University A").ToList();

        // Assert
        sameUniMatches.Should().HaveCount(2);
    }

    private static List<UserMatchDto> CreateSampleMatches()
    {
        return new List<UserMatchDto>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "match1@test.com", "Match 1", 24, "Female", "University A", "Bio 1", "social", "Sports", DateTime.UtcNow.AddDays(-1), true),
            new(Guid.NewGuid(), Guid.NewGuid(), "match2@test.com", "Match 2", 26, "Male", "University B", "Bio 2", "quiet", "Music", DateTime.UtcNow.AddDays(-3), true),
            new(Guid.NewGuid(), Guid.NewGuid(), "match3@test.com", "Match 3", 23, "Female", "University A", "Bio 3", "active", "Travel", DateTime.UtcNow.AddDays(-7), true),
            new(Guid.NewGuid(), Guid.NewGuid(), "match4@test.com", "Match 4", 25, "Male", "University C", "Bio 4", "studious", "Reading", DateTime.UtcNow.AddDays(-14), false)
        };
    }

    #endregion

    #region Conversations Sorting Tests

    [Fact]
    public void Conversations_SortByCreatedAt_Descending_ShouldWork()
    {
        // Arrange
        var conversations = CreateSampleConversations();

        // Act
        var sorted = conversations.OrderByDescending(c => c.CreatedAt).ToList();

        // Assert
        sorted.First().CreatedAt.Should().BeAfter(sorted.Last().CreatedAt);
    }

    [Fact]
    public void Conversations_FilterByRole_ShouldWork()
    {
        // Arrange
        var conversations = CreateSampleConversations();

        // Act
        var adminConversations = conversations.Where(c => c.OtherUserRole == "Admin").ToList();

        // Assert
        adminConversations.Should().HaveCount(1);
    }

    private static List<ConversationDto> CreateSampleConversations()
    {
        return new List<ConversationDto>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "User 1", "/images/user1.jpg", "User", DateTime.UtcNow.AddHours(-1)),
            new(Guid.NewGuid(), Guid.NewGuid(), "User 2", null, "User", DateTime.UtcNow.AddDays(-1)),
            new(Guid.NewGuid(), Guid.NewGuid(), "Admin User", "/images/admin.jpg", "Admin", DateTime.UtcNow.AddDays(-2)),
            new(Guid.NewGuid(), Guid.NewGuid(), "User 3", null, "User", DateTime.UtcNow.AddDays(-5))
        };
    }

    #endregion

    #region Messages Sorting Tests

    [Fact]
    public void Messages_SortBySentAt_ShouldWork()
    {
        // Arrange
        var messages = CreateSampleMessages();

        // Act
        var sorted = messages.OrderBy(m => m.SentAt).ToList();

        // Assert - oldest first
        sorted.First().SentAt.Should().BeBefore(sorted.Last().SentAt);
    }

    [Fact]
    public void Messages_FilterByUnread_ShouldWork()
    {
        // Arrange
        var messages = CreateSampleMessages();

        // Act
        var unreadMessages = messages.Where(m => !m.IsRead).ToList();

        // Assert
        unreadMessages.Should().HaveCount(2);
    }

    [Fact]
    public void Messages_CountBySender_ShouldWork()
    {
        // Arrange
        var messages = CreateSampleMessages();
        var senderId = messages.First().SenderId;

        // Act
        var senderMessages = messages.Where(m => m.SenderId == senderId).ToList();

        // Assert
        senderMessages.Count.Should().BeGreaterThan(0);
    }

    private static List<MessageDto> CreateSampleMessages()
    {
        var senderId1 = Guid.NewGuid();
        var senderId2 = Guid.NewGuid();

        return new List<MessageDto>
        {
            new(Guid.NewGuid(), senderId1, "User 1", "User", "Hello!", DateTime.UtcNow.AddMinutes(-30), true),
            new(Guid.NewGuid(), senderId2, "User 2", "User", "Hi there!", DateTime.UtcNow.AddMinutes(-25), true),
            new(Guid.NewGuid(), senderId1, "User 1", "User", "How are you?", DateTime.UtcNow.AddMinutes(-20), true),
            new(Guid.NewGuid(), senderId2, "User 2", "User", "I'm good, thanks!", DateTime.UtcNow.AddMinutes(-15), false),
            new(Guid.NewGuid(), senderId1, "User 1", "User", "Great!", DateTime.UtcNow.AddMinutes(-10), false)
        };
    }

    #endregion

    #region Reviews Statistics Tests

    [Fact]
    public void Reviews_CalculateAverageRating_ShouldWork()
    {
        // Arrange
        var reviews = CreateSampleReviews();

        // Act
        var averageRating = reviews.Average(r => r.Rating);

        // Assert
        averageRating.Should().BeApproximately(3.8, 0.1);
    }

    [Fact]
    public void Reviews_CountByRating_ShouldWork()
    {
        // Arrange
        var reviews = CreateSampleReviews();

        // Act
        var ratingCounts = reviews.GroupBy(r => r.Rating)
            .ToDictionary(g => g.Key, g => g.Count());

        // Assert
        ratingCounts.Should().ContainKey(5);
        ratingCounts.Should().ContainKey(4);
    }

    [Fact]
    public void Reviews_SortByCreatedAt_ShouldWork()
    {
        // Arrange
        var reviews = CreateSampleReviews();

        // Act
        var sorted = reviews.OrderByDescending(r => r.CreatedAt).ToList();

        // Assert
        sorted.First().CreatedAt.Should().BeAfter(sorted.Last().CreatedAt);
    }

    private static List<Review> CreateSampleReviews()
    {
        return new List<Review>
        {
            new() { Id = Guid.NewGuid(), ReviewerId = Guid.NewGuid(), ReviewerFullName = "Reviewer 1", Rating = 5, Comment = "Excellent!", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new() { Id = Guid.NewGuid(), ReviewerId = Guid.NewGuid(), ReviewerFullName = "Reviewer 2", Rating = 4, Comment = "Very good", CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new() { Id = Guid.NewGuid(), ReviewerId = Guid.NewGuid(), ReviewerFullName = "Reviewer 3", Rating = 3, Comment = "Average", CreatedAt = DateTime.UtcNow.AddDays(-7) },
            new() { Id = Guid.NewGuid(), ReviewerId = Guid.NewGuid(), ReviewerFullName = "Reviewer 4", Rating = 4, Comment = "Good experience", CreatedAt = DateTime.UtcNow.AddDays(-10) },
            new() { Id = Guid.NewGuid(), ReviewerId = Guid.NewGuid(), ReviewerFullName = "Reviewer 5", Rating = 3, Comment = "OK", CreatedAt = DateTime.UtcNow.AddDays(-14) }
        };
    }

    #endregion

    #region Pagination Tests

    [Theory]
    [InlineData(100, 10, 10)]
    [InlineData(95, 10, 10)]
    [InlineData(10, 10, 1)]
    [InlineData(0, 10, 0)]
    [InlineData(25, 5, 5)]
    public void Pagination_TotalPages_ShouldBeCalculatedCorrectly(int totalItems, int pageSize, int expectedPages)
    {
        // Act
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling((double)totalItems / pageSize);

        // Assert
        totalPages.Should().Be(expectedPages);
    }

    [Theory]
    [InlineData(1, 10, 0)]
    [InlineData(2, 10, 10)]
    [InlineData(3, 10, 20)]
    [InlineData(1, 20, 0)]
    [InlineData(5, 5, 20)]
    public void Pagination_Skip_ShouldBeCalculatedCorrectly(int page, int pageSize, int expectedSkip)
    {
        // Act
        var skip = (page - 1) * pageSize;

        // Assert
        skip.Should().Be(expectedSkip);
    }

    #endregion
}

