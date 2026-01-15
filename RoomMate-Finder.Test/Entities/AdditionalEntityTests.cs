using FluentAssertions;
using RoomMate_Finder.Entities;

namespace RoomMate_Finder.Test.Entities;

/// <summary>
/// Additional entity tests for increased coverage.
/// </summary>
public class AdditionalEntityTests
{
    #region Profile Additional Tests

    [Fact]
    public void Given_Profile_When_AllPropertiesSet_Then_AllPropertiesStored()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var profile = new Profile
        {
            Id = id,
            Email = "complete@test.com",
            PasswordHash = "hashedpassword123",
            FullName = "Complete User",
            Age = 30,
            Gender = "Male",
            ProfilePicturePath = "/uploads/picture.jpg",
            University = "MIT",
            Bio = "Software developer with 10 years experience",
            Lifestyle = "Night Owl, Clean, Quiet",
            Interests = "Coding, Gaming, Reading",
            Role = "Admin",
            CreatedAt = createdAt
        };

        // Assert
        profile.Id.Should().Be(id);
        profile.Email.Should().Be("complete@test.com");
        profile.PasswordHash.Should().Be("hashedpassword123");
        profile.FullName.Should().Be("Complete User");
        profile.Age.Should().Be(30);
        profile.Gender.Should().Be("Male");
        profile.ProfilePicturePath.Should().Be("/uploads/picture.jpg");
        profile.University.Should().Be("MIT");
        profile.Bio.Should().Be("Software developer with 10 years experience");
        profile.Lifestyle.Should().Be("Night Owl, Clean, Quiet");
        profile.Interests.Should().Be("Coding, Gaming, Reading");
        profile.Role.Should().Be("Admin");
        profile.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void Given_Profile_When_NullablePropertiesNotSet_Then_DefaultsToNull()
    {
        // Arrange & Act
        var profile = new Profile();

        // Assert - ProfilePicturePath is nullable, so it defaults to null
        profile.ProfilePicturePath.Should().BeNull();
    }

    [Theory]
    [InlineData("User")]
    [InlineData("Admin")]
    [InlineData("Moderator")]
    public void Given_Profile_When_RoleSet_Then_RoleStored(string role)
    {
        // Arrange & Act
        var profile = new Profile { Role = role };

        // Assert
        profile.Role.Should().Be(role);
    }

    #endregion

    #region Conversation Additional Tests

    [Fact]
    public void Given_Conversation_When_AllPropertiesSet_Then_AllPropertiesStored()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var user1 = new Profile { Id = user1Id, Email = "user1@test.com" };
        var user2 = new Profile { Id = user2Id, Email = "user2@test.com" };

        // Act
        var conversation = new Conversation
        {
            Id = id,
            User1Id = user1Id,
            User2Id = user2Id,
            CreatedAt = createdAt,
            User1 = user1,
            User2 = user2
        };

        // Assert
        conversation.Id.Should().Be(id);
        conversation.User1Id.Should().Be(user1Id);
        conversation.User2Id.Should().Be(user2Id);
        conversation.CreatedAt.Should().Be(createdAt);
        conversation.User1.Should().Be(user1);
        conversation.User2.Should().Be(user2);
    }

    #endregion

    #region Message Additional Tests

    [Fact]
    public void Given_Message_When_AllPropertiesSet_Then_AllPropertiesStored()
    {
        // Arrange
        var id = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var sentAt = DateTime.UtcNow;

        // Act
        var message = new Message
        {
            Id = id,
            ConversationId = conversationId,
            SenderId = senderId,
            Content = "Test message content",
            SentAt = sentAt,
            IsRead = true
        };

        // Assert
        message.Id.Should().Be(id);
        message.ConversationId.Should().Be(conversationId);
        message.SenderId.Should().Be(senderId);
        message.Content.Should().Be("Test message content");
        message.SentAt.Should().Be(sentAt);
        message.IsRead.Should().BeTrue();
    }

    [Fact]
    public void Given_Message_When_DefaultIsRead_Then_IsFalse()
    {
        // Arrange & Act
        var message = new Message();

        // Assert
        message.IsRead.Should().BeFalse();
    }

    #endregion

    #region RoomListing Additional Tests

    [Fact]
    public void Given_RoomListing_When_AllPropertiesSet_Then_AllPropertiesStored()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var availableFrom = DateTime.UtcNow.AddDays(30);
        var approvedAt = DateTime.UtcNow.AddDays(1);
        var adminId = Guid.NewGuid();

        // Act
        var listing = new RoomListing
        {
            Id = id,
            OwnerId = ownerId,
            Title = "Cozy Room for Rent",
            Description = "Beautiful room in city center",
            City = "Cluj-Napoca",
            Area = "Marasti",
            Price = 350,
            AvailableFrom = availableFrom,
            Amenities = "WiFi, AC, Washing Machine",
            IsActive = true,
            CreatedAt = createdAt,
            ImagePaths = "path1.jpg,path2.jpg",
            ApprovalStatus = ListingApprovalStatus.Approved,
            ApprovedAt = approvedAt,
            ApprovedByAdminId = adminId,
            RejectionReason = null
        };

        // Assert
        listing.Id.Should().Be(id);
        listing.OwnerId.Should().Be(ownerId);
        listing.Title.Should().Be("Cozy Room for Rent");
        listing.Description.Should().Be("Beautiful room in city center");
        listing.City.Should().Be("Cluj-Napoca");
        listing.Area.Should().Be("Marasti");
        listing.Price.Should().Be(350);
        listing.AvailableFrom.Should().Be(availableFrom);
        listing.Amenities.Should().Be("WiFi, AC, Washing Machine");
        listing.IsActive.Should().BeTrue();
        listing.CreatedAt.Should().Be(createdAt);
        listing.ImagePaths.Should().Be("path1.jpg,path2.jpg");
        listing.ApprovalStatus.Should().Be(ListingApprovalStatus.Approved);
        listing.ApprovedAt.Should().Be(approvedAt);
        listing.ApprovedByAdminId.Should().Be(adminId);
        listing.RejectionReason.Should().BeNull();
    }

    [Fact]
    public void Given_RoomListing_When_Rejected_Then_RejectionReasonStored()
    {
        // Arrange & Act
        var listing = new RoomListing
        {
            ApprovalStatus = ListingApprovalStatus.Rejected,
            RejectionReason = "Inappropriate content"
        };

        // Assert
        listing.ApprovalStatus.Should().Be(ListingApprovalStatus.Rejected);
        listing.RejectionReason.Should().Be("Inappropriate content");
    }

    [Fact]
    public void Given_RoomListing_When_ImagesAdded_Then_ImagesStored()
    {
        // Arrange
        var listing = new RoomListing();
        var image1 = new RoomListingImage { Id = Guid.NewGuid(), ImagePath = "img1.jpg" };
        var image2 = new RoomListingImage { Id = Guid.NewGuid(), ImagePath = "img2.jpg" };

        // Act
        listing.Images = new List<RoomListingImage> { image1, image2 };

        // Assert
        listing.Images.Should().HaveCount(2);
    }

    #endregion

    #region Review Additional Tests

    [Fact]
    public void Given_Review_When_AllPropertiesSet_Then_AllPropertiesStored()
    {
        // Arrange
        var id = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var reviewedUserId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var review = new Review
        {
            Id = id,
            ReviewerId = reviewerId,
            ReviewedUserId = reviewedUserId,
            Rating = 5,
            Comment = "Great roommate, highly recommended!",
            CreatedAt = createdAt
        };

        // Assert
        review.Id.Should().Be(id);
        review.ReviewerId.Should().Be(reviewerId);
        review.ReviewedUserId.Should().Be(reviewedUserId);
        review.Rating.Should().Be(5);
        review.Comment.Should().Be("Great roommate, highly recommended!");
        review.CreatedAt.Should().Be(createdAt);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Given_Review_When_RatingSet_Then_RatingStored(int rating)
    {
        // Arrange & Act
        var review = new Review { Rating = rating };

        // Assert
        review.Rating.Should().Be(rating);
    }

    #endregion

    #region Match Additional Tests

    [Fact]
    public void Given_Match_When_AllPropertiesSet_Then_AllPropertiesStored()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var match = new Match
        {
            Id = id,
            User1Id = user1Id,
            User2Id = user2Id,
            CreatedAt = createdAt,
            IsActive = true
        };

        // Assert
        match.Id.Should().Be(id);
        match.User1Id.Should().Be(user1Id);
        match.User2Id.Should().Be(user2Id);
        match.CreatedAt.Should().Be(createdAt);
        match.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Given_Match_When_Deactivated_Then_IsActiveFalse()
    {
        // Arrange
        var match = new Match { IsActive = true };

        // Act
        match.IsActive = false;

        // Assert
        match.IsActive.Should().BeFalse();
    }

    #endregion

    #region UserAction Additional Tests

    [Fact]
    public void Given_UserAction_When_AllPropertiesSet_Then_AllPropertiesStored()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var action = new UserAction
        {
            Id = id,
            UserId = userId,
            TargetUserId = targetUserId,
            ActionType = ActionType.Like,
            CreatedAt = createdAt
        };

        // Assert
        action.Id.Should().Be(id);
        action.UserId.Should().Be(userId);
        action.TargetUserId.Should().Be(targetUserId);
        action.ActionType.Should().Be(ActionType.Like);
        action.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void Given_UserAction_When_Pass_Then_ActionTypeIsPass()
    {
        // Arrange & Act
        var action = new UserAction { ActionType = ActionType.Pass };

        // Assert
        action.ActionType.Should().Be(ActionType.Pass);
    }

    #endregion

    #region RoommateRelationship Additional Tests

    [Fact]
    public void Given_RoommateRelationship_When_AllPropertiesSet_Then_AllPropertiesStored()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var relationship = new RoommateRelationship
        {
            Id = id,
            User1Id = user1Id,
            User2Id = user2Id,
            ApprovedByAdminId = adminId,
            OriginalRequestId = requestId,
            CreatedAt = createdAt,
            IsActive = true
        };

        // Assert
        relationship.Id.Should().Be(id);
        relationship.User1Id.Should().Be(user1Id);
        relationship.User2Id.Should().Be(user2Id);
        relationship.ApprovedByAdminId.Should().Be(adminId);
        relationship.OriginalRequestId.Should().Be(requestId);
        relationship.CreatedAt.Should().Be(createdAt);
        relationship.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Given_RoommateRelationship_When_NoOriginalRequest_Then_NullableIdIsNull()
    {
        // Arrange & Act
        var relationship = new RoommateRelationship
        {
            OriginalRequestId = null
        };

        // Assert
        relationship.OriginalRequestId.Should().BeNull();
    }

    #endregion

    #region RoommateRequest Additional Tests

    [Fact]
    public void Given_RoommateRequest_When_AllPropertiesSet_Then_AllPropertiesStored()
    {
        // Arrange
        var id = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var processedAt = DateTime.UtcNow.AddHours(1);

        // Act
        var request = new RoommateRequest
        {
            Id = id,
            RequesterId = requesterId,
            TargetUserId = targetId,
            Status = RoommateRequestStatus.MutuallyConfirmed,
            Message = "Please consider my request",
            CreatedAt = createdAt,
            ProcessedAt = processedAt,
            ProcessedByAdminId = adminId
        };

        // Assert
        request.Id.Should().Be(id);
        request.RequesterId.Should().Be(requesterId);
        request.TargetUserId.Should().Be(targetId);
        request.Status.Should().Be(RoommateRequestStatus.MutuallyConfirmed);
        request.Message.Should().Be("Please consider my request");
        request.CreatedAt.Should().Be(createdAt);
        request.ProcessedAt.Should().Be(processedAt);
        request.ProcessedByAdminId.Should().Be(adminId);
    }

    [Fact]
    public void Given_RoommateRequest_When_NotProcessed_Then_ProcessedAtIsNull()
    {
        // Arrange & Act
        var request = new RoommateRequest();

        // Assert
        request.ProcessedAt.Should().BeNull();
        request.ProcessedByAdminId.Should().BeNull();
    }

    [Theory]
    [InlineData(RoommateRequestStatus.Pending)]
    [InlineData(RoommateRequestStatus.MutuallyConfirmed)]
    [InlineData(RoommateRequestStatus.Approved)]
    [InlineData(RoommateRequestStatus.Rejected)]
    public void Given_RoommateRequest_When_StatusSet_Then_StatusStored(RoommateRequestStatus status)
    {
        // Arrange & Act
        var request = new RoommateRequest { Status = status };

        // Assert
        request.Status.Should().Be(status);
    }

    #endregion

    #region RoomListingImage Additional Tests

    [Fact]
    public void Given_RoomListingImage_When_AllPropertiesSet_Then_AllPropertiesStored()
    {
        // Arrange
        var id = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        // Act
        var image = new RoomListingImage
        {
            Id = id,
            RoomListingId = listingId,
            ImagePath = "/uploads/listings/room1.jpg",
            DisplayOrder = 1
        };

        // Assert
        image.Id.Should().Be(id);
        image.RoomListingId.Should().Be(listingId);
        image.ImagePath.Should().Be("/uploads/listings/room1.jpg");
        image.DisplayOrder.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Given_RoomListingImage_When_DisplayOrderSet_Then_DisplayOrderStored(int displayOrder)
    {
        // Arrange & Act
        var image = new RoomListingImage { DisplayOrder = displayOrder };

        // Assert
        image.DisplayOrder.Should().Be(displayOrder);
    }

    #endregion
}
