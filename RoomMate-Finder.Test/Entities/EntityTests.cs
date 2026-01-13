using FluentAssertions;
using RoomMate_Finder.Entities;

namespace RoomMate_Finder.Test.Entities;

public class ProfileEntityTests
{
    [Fact]
    public void Given_NewProfile_When_Created_Then_HasDefaultValues()
    {
        var profile = new Profile();
        profile.Id.Should().Be(Guid.Empty);
        profile.Email.Should().BeEmpty();
        profile.Role.Should().Be("User");
    }

    [Fact]
    public void Given_Profile_When_SetProperties_Then_PropertiesAreSet()
    {
        var id = Guid.NewGuid();
        var profile = new Profile
        {
            Id = id,
            Email = "test@example.com",
            FullName = "Test User",
            Role = "Admin"
        };
        profile.Id.Should().Be(id);
        profile.Email.Should().Be("test@example.com");
        profile.Role.Should().Be("Admin");
    }
}

public class ConversationEntityTests
{
    [Fact]
    public void Given_NewConversation_When_Created_Then_HasDefaultValues()
    {
        var conversation = new Conversation();
        conversation.Id.Should().Be(Guid.Empty);
        conversation.User1Id.Should().Be(Guid.Empty);
        conversation.User2Id.Should().Be(Guid.Empty);
    }
}

public class MessageEntityTests
{
    [Fact]
    public void Given_NewMessage_When_Created_Then_HasDefaultValues()
    {
        var message = new Message();
        message.Id.Should().Be(Guid.Empty);
        message.Content.Should().BeEmpty();
        message.IsRead.Should().BeFalse();
    }

    [Fact]
    public void Given_Message_When_MarkAsRead_Then_IsReadIsTrue()
    {
        var message = new Message { IsRead = false };
        message.IsRead = true;
        message.IsRead.Should().BeTrue();
    }
}

public class ReviewEntityTests
{
    [Fact]
    public void Given_NewReview_When_Created_Then_HasDefaultValues()
    {
        var review = new Review();
        review.Id.Should().Be(Guid.Empty);
        review.Rating.Should().Be(0);
        review.Comment.Should().BeEmpty();
    }
}

public class UserActionEntityTests
{
    [Fact]
    public void Given_ActionType_When_Like_Then_ValueIs1()
    {
        ((int)ActionType.Like).Should().Be(1);
    }

    [Fact]
    public void Given_ActionType_When_Pass_Then_ValueIs2()
    {
        ((int)ActionType.Pass).Should().Be(2);
    }
}

public class MatchEntityTests
{
    [Fact]
    public void Given_NewMatch_When_Created_Then_IsActiveIsTrue()
    {
        var match = new Match();
        match.IsActive.Should().BeTrue();
    }
}

public class RoomListingEntityTests
{
    [Fact]
    public void Given_NewRoomListing_When_Created_Then_HasDefaultValues()
    {
        var listing = new RoomListing();
        listing.Id.Should().Be(Guid.Empty);
        listing.IsActive.Should().BeTrue();
        listing.ApprovalStatus.Should().Be(ListingApprovalStatus.Pending);
    }

    [Fact]
    public void Given_ListingApprovalStatus_Values_Then_CorrectEnumValues()
    {
        ((int)ListingApprovalStatus.Pending).Should().Be(0);
        ((int)ListingApprovalStatus.Approved).Should().Be(1);
        ((int)ListingApprovalStatus.Rejected).Should().Be(2);
    }
}

public class RoommateRequestEntityTests
{
    [Fact]
    public void Given_NewRoommateRequest_When_Created_Then_HasDefaultValues()
    {
        var request = new RoommateRequest();
        request.Id.Should().Be(Guid.Empty);
        request.Status.Should().Be(RoommateRequestStatus.Pending);
        request.Message.Should().BeNull();
    }

    [Fact]
    public void Given_RoommateRequestStatus_Values_Then_CorrectEnumValues()
    {
        ((int)RoommateRequestStatus.Pending).Should().Be(0);
        ((int)RoommateRequestStatus.MutuallyConfirmed).Should().Be(1);
        ((int)RoommateRequestStatus.Approved).Should().Be(2);
        ((int)RoommateRequestStatus.Rejected).Should().Be(3);
    }
}

public class RoommateRelationshipEntityTests
{
    [Fact]
    public void Given_NewRoommateRelationship_When_Created_Then_HasDefaultValues()
    {
        var relationship = new RoommateRelationship();
        relationship.Id.Should().Be(Guid.Empty);
        relationship.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Given_ActiveRelationship_When_Deactivated_Then_IsActiveBecomesFalse()
    {
        var relationship = new RoommateRelationship { IsActive = true };
        relationship.IsActive = false;
        relationship.IsActive.Should().BeFalse();
    }
}

