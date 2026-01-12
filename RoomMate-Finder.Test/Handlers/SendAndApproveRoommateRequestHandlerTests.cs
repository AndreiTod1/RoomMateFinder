using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Roommates.ApproveRequest;
using RoomMate_Finder.Features.Roommates.SendRequest;
using RoomMate_Finder.Test.Helpers;
using System.Security.Claims;

namespace RoomMate_Finder.Test.Handlers;

public class SendAndApproveRoommateRequestHandlerTests
{
    private static IHttpContextAccessor CreateHttpContextAccessor(Guid userId)
    {
        var mockAccessor = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);
        
        mockAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        return mockAccessor.Object;
    }

    #region SendRoommateRequestHandler Tests

    [Fact]
    public async Task Given_TargetUserNotFound_When_SendRequestIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var requesterId = Guid.NewGuid();
        var handler = new SendRoommateRequestHandler(context, CreateHttpContextAccessor(requesterId));
        
        var request = new SendRoommateRequestRequest(Guid.NewGuid(), "Want to be roommates?");

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Target user not found*");
    }

    [Fact]
    public async Task Given_RequestToSelf_When_SendRequestIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var userId = Guid.NewGuid();
        
        var user = new Profile
        {
            Id = userId,
            Email = "user@test.com",
            PasswordHash = "hash",
            FullName = "Test User",
            Age = 25,
            Gender = "M",
            CreatedAt = DateTime.UtcNow
        };
        context.Profiles.Add(user);
        await context.SaveChangesAsync();

        var handler = new SendRoommateRequestHandler(context, CreateHttpContextAccessor(userId));
        var request = new SendRoommateRequestRequest(userId, "Request to myself");

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot send a roommate request to yourself*");
    }

    [Fact]
    public async Task Given_ExistingPendingRequest_When_SendRequestIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var requesterId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        
        var requester = new Profile
        {
            Id = requesterId,
            Email = "requester@test.com",
            PasswordHash = "hash",
            FullName = "Requester",
            Age = 25,
            Gender = "M",
            CreatedAt = DateTime.UtcNow
        };
        var target = new Profile
        {
            Id = targetId,
            Email = "target@test.com",
            PasswordHash = "hash",
            FullName = "Target",
            Age = 26,
            Gender = "F",
            CreatedAt = DateTime.UtcNow
        };
        var existingRequest = new RoommateRequest
        {
            Id = Guid.NewGuid(),
            RequesterId = requesterId,
            TargetUserId = targetId,
            Status = RoommateRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Profiles.AddRange(requester, target);
        context.RoommateRequests.Add(existingRequest);
        await context.SaveChangesAsync();

        var handler = new SendRoommateRequestHandler(context, CreateHttpContextAccessor(requesterId));
        var request = new SendRoommateRequestRequest(targetId, "Another request");

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already have a pending request*");
    }

    [Fact]
    public async Task Given_ExistingActiveRelationship_When_SendRequestIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var requesterId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        
        var requester = new Profile
        {
            Id = requesterId,
            Email = "requester@test.com",
            PasswordHash = "hash",
            FullName = "Requester",
            Age = 25,
            Gender = "M",
            CreatedAt = DateTime.UtcNow
        };
        var target = new Profile
        {
            Id = targetId,
            Email = "target@test.com",
            PasswordHash = "hash",
            FullName = "Target",
            Age = 26,
            Gender = "F",
            CreatedAt = DateTime.UtcNow
        };
        var existingRelationship = new RoommateRelationship
        {
            Id = Guid.NewGuid(),
            User1Id = requesterId,
            User2Id = targetId,
            ApprovedByAdminId = Guid.NewGuid(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Profiles.AddRange(requester, target);
        context.RoommateRelationships.Add(existingRelationship);
        await context.SaveChangesAsync();

        var handler = new SendRoommateRequestHandler(context, CreateHttpContextAccessor(requesterId));
        var request = new SendRoommateRequestRequest(targetId, "New request");

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already have an active roommate relationship*");
    }

    [Fact]
    public async Task Given_ValidNewRequest_When_SendRequestIsCalled_Then_CreatesRequestWithPendingStatus()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var requesterId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        
        var requester = new Profile
        {
            Id = requesterId,
            Email = "requester@test.com",
            PasswordHash = "hash",
            FullName = "Requester",
            Age = 25,
            Gender = "M",
            CreatedAt = DateTime.UtcNow
        };
        var target = new Profile
        {
            Id = targetId,
            Email = "target@test.com",
            PasswordHash = "hash",
            FullName = "Target",
            Age = 26,
            Gender = "F",
            CreatedAt = DateTime.UtcNow
        };
        
        context.Profiles.AddRange(requester, target);
        await context.SaveChangesAsync();

        var handler = new SendRoommateRequestHandler(context, CreateHttpContextAccessor(requesterId));
        var request = new SendRoommateRequestRequest(targetId, "Want to be roommates?");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.Message.Should().Contain("sent successfully");

        var savedRequest = await context.RoommateRequests.FindAsync(result.Id);
        savedRequest!.Status.Should().Be(RoommateRequestStatus.Pending);
    }

    [Fact]
    public async Task Given_InverseRequestExists_When_SendRequestIsCalled_Then_BothRequestsAreMutuallyConfirmed()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var requesterId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        
        var requester = new Profile
        {
            Id = requesterId,
            Email = "requester@test.com",
            PasswordHash = "hash",
            FullName = "Requester",
            Age = 25,
            Gender = "M",
            CreatedAt = DateTime.UtcNow
        };
        var target = new Profile
        {
            Id = targetId,
            Email = "target@test.com",
            PasswordHash = "hash",
            FullName = "Target",
            Age = 26,
            Gender = "F",
            CreatedAt = DateTime.UtcNow
        };
        
        // Target already sent a request to requester
        var inverseRequest = new RoommateRequest
        {
            Id = Guid.NewGuid(),
            RequesterId = targetId,
            TargetUserId = requesterId,
            Status = RoommateRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Profiles.AddRange(requester, target);
        context.RoommateRequests.Add(inverseRequest);
        await context.SaveChangesAsync();

        var handler = new SendRoommateRequestHandler(context, CreateHttpContextAccessor(requesterId));
        var request = new SendRoommateRequestRequest(targetId, "I confirm!");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Message.Should().Contain("Both users have confirmed");
        
        var updatedInverse = await context.RoommateRequests.FindAsync(inverseRequest.Id);
        updatedInverse!.Status.Should().Be(RoommateRequestStatus.MutuallyConfirmed);
    }

    #endregion

    #region ApproveRequestHandler Tests

    [Fact]
    public async Task Given_RequestNotFound_When_ApproveRequestIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var adminId = Guid.NewGuid();
        var handler = new ApproveRequestHandler(context, CreateHttpContextAccessor(adminId));
        
        var request = new ApproveRequestRequest(Guid.NewGuid());

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Request not found*");
    }

    [Fact]
    public async Task Given_RequestNotMutuallyConfirmed_When_ApproveRequestIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var adminId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        
        var requester = new Profile
        {
            Id = requesterId,
            Email = "requester@test.com",
            PasswordHash = "hash",
            FullName = "Requester",
            Age = 25,
            Gender = "M",
            CreatedAt = DateTime.UtcNow
        };
        var target = new Profile
        {
            Id = targetId,
            Email = "target@test.com",
            PasswordHash = "hash",
            FullName = "Target",
            Age = 26,
            Gender = "F",
            CreatedAt = DateTime.UtcNow
        };
        
        var roommateRequest = new RoommateRequest
        {
            Id = Guid.NewGuid(),
            RequesterId = requesterId,
            TargetUserId = targetId,
            Status = RoommateRequestStatus.Pending, // Not mutually confirmed
            CreatedAt = DateTime.UtcNow
        };
        
        context.Profiles.AddRange(requester, target);
        context.RoommateRequests.Add(roommateRequest);
        await context.SaveChangesAsync();

        var handler = new ApproveRequestHandler(context, CreateHttpContextAccessor(adminId));
        var request = new ApproveRequestRequest(roommateRequest.Id);

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not been mutually confirmed*");
    }

    [Fact]
    public async Task Given_MutuallyConfirmedRequest_When_ApproveRequestIsCalled_Then_CreatesRelationship()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var adminId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        
        var requester = new Profile
        {
            Id = requesterId,
            Email = "requester@test.com",
            PasswordHash = "hash",
            FullName = "Requester",
            Age = 25,
            Gender = "M",
            CreatedAt = DateTime.UtcNow
        };
        var target = new Profile
        {
            Id = targetId,
            Email = "target@test.com",
            PasswordHash = "hash",
            FullName = "Target",
            Age = 26,
            Gender = "F",
            CreatedAt = DateTime.UtcNow
        };
        
        var roommateRequest = new RoommateRequest
        {
            Id = Guid.NewGuid(),
            RequesterId = requesterId,
            TargetUserId = targetId,
            Status = RoommateRequestStatus.MutuallyConfirmed,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Profiles.AddRange(requester, target);
        context.RoommateRequests.Add(roommateRequest);
        await context.SaveChangesAsync();

        var handler = new ApproveRequestHandler(context, CreateHttpContextAccessor(adminId));
        var request = new ApproveRequestRequest(roommateRequest.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.RelationshipId.Should().NotBe(Guid.Empty);
        result.Message.Should().Contain("approved");

        var relationship = await context.RoommateRelationships.FindAsync(result.RelationshipId);
        relationship!.IsActive.Should().BeTrue();
        relationship.ApprovedByAdminId.Should().Be(adminId);
    }

    #endregion
}
