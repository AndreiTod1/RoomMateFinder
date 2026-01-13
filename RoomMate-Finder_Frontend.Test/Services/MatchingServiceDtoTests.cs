using FluentAssertions;
using RoomMate_Finder_Frontend.Services;

namespace RoomMate_Finder_Frontend.Test.Services;

public class MatchingServiceDtoTests
{
    #region IMatchingService Interface Contract Tests

    [Fact]
    public void IMatchingService_Interface_ShouldExist()
    {
        // Assert
        typeof(IMatchingService).Should().NotBeNull();
        typeof(IMatchingService).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IMatchingService_Should_HaveGetDiscoverProfilesAsyncMethod()
    {
        // Assert
        var method = typeof(IMatchingService).GetMethod("GetDiscoverProfilesAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void IMatchingService_Should_HaveCalculateCompatibilityAsyncMethod()
    {
        // Assert
        var method = typeof(IMatchingService).GetMethod("CalculateCompatibilityAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void IMatchingService_Should_HaveGetMyMatchesAsyncMethod()
    {
        // Assert
        var method = typeof(IMatchingService).GetMethod("GetMyMatchesAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void IMatchingService_Should_HaveLikeProfileAsyncMethod()
    {
        // Assert
        var method = typeof(IMatchingService).GetMethod("LikeProfileAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void IMatchingService_Should_HavePassProfileAsyncMethod()
    {
        // Assert
        var method = typeof(IMatchingService).GetMethod("PassProfileAsync");
        method.Should().NotBeNull();
    }

    #endregion

    #region IReviewService Interface Contract Tests

    [Fact]
    public void IReviewService_Interface_ShouldExist()
    {
        // Assert
        typeof(IReviewService).Should().NotBeNull();
        typeof(IReviewService).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IReviewService_Should_HaveGetMatchesForReviewMethod()
    {
        // Assert
        var method = typeof(IReviewService).GetMethod("GetMatchesForReview");
        method.Should().NotBeNull();
    }

    [Fact]
    public void IReviewService_Should_HaveLeaveReviewAsyncMethod()
    {
        // Assert
        var method = typeof(IReviewService).GetMethod("LeaveReviewAsync");
        method.Should().NotBeNull();
    }

    #endregion

    #region IRoommateService Interface Contract Tests

    [Fact]
    public void IRoommateService_Interface_ShouldExist()
    {
        // Assert
        typeof(IRoommateService).Should().NotBeNull();
        typeof(IRoommateService).IsInterface.Should().BeTrue();
    }

    [Fact]
    public void IRoommateService_Should_HaveSendRoommateRequestAsyncMethod()
    {
        // Assert
        var method = typeof(IRoommateService).GetMethod("SendRoommateRequestAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void IRoommateService_Should_HaveGetMyRequestsAsyncMethod()
    {
        // Assert
        var method = typeof(IRoommateService).GetMethod("GetMyRequestsAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public void IRoommateService_Should_HaveAdminEndpoints()
    {
        // Assert
        typeof(IRoommateService).GetMethod("GetPendingRequestsAsync").Should().NotBeNull();
        typeof(IRoommateService).GetMethod("ApproveRequestAsync").Should().NotBeNull();
        typeof(IRoommateService).GetMethod("RejectRequestAsync").Should().NotBeNull();
        typeof(IRoommateService).GetMethod("GetRelationshipsAsync").Should().NotBeNull();
        typeof(IRoommateService).GetMethod("DeleteRelationshipAsync").Should().NotBeNull();
    }

    #endregion
}

