using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Roommates.DeleteRelationship;
using RoomMate_Finder.Features.Roommates.RejectRequest;
using RoomMate_Finder.Test.Helpers;
using System.Security.Claims;

namespace RoomMate_Finder.Test.Handlers;

public class RoommateActionHandlersTests
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

    #region RejectRequestHandler Tests

    [Fact]
    public async Task Given_NoAdminAuth_When_RejectCalled_Then_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null!); 
        
        var handler = new RejectRequestHandler(context, mockAccessor.Object);

        // Act
        Func<Task> act = () => handler.Handle(new RejectRequestRequest(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Given_RequestNotFound_When_RejectCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var adminId = Guid.NewGuid();
        var handler = new RejectRequestHandler(context, CreateHttpContextAccessor(adminId));

        // Act
        Func<Task> act = () => handler.Handle(new RejectRequestRequest(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Given_AlreadyProcessedRequest_When_RejectCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var adminId = Guid.NewGuid();
        
        var request = new RoommateRequest
        {
            Id = Guid.NewGuid(),
            RequesterId = Guid.NewGuid(),
            TargetUserId = Guid.NewGuid(),
            Status = RoommateRequestStatus.Approved, // Already processed
            CreatedAt = DateTime.UtcNow
        };
        context.RoommateRequests.Add(request);
        await context.SaveChangesAsync();

        var handler = new RejectRequestHandler(context, CreateHttpContextAccessor(adminId));

        // Act
        Func<Task> act = () => handler.Handle(new RejectRequestRequest(request.Id), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already been processed*");
    }

    [Fact]
    public async Task Given_ValidPendingRequest_When_RejectCalled_Then_RejectsRequestAndInverseAndSetsAdminId()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var adminId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        
        var request = new RoommateRequest
        {
            Id = Guid.NewGuid(),
            RequesterId = requesterId,
            TargetUserId = targetId,
            Status = RoommateRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        // Inverse request
        var inverseRequest = new RoommateRequest
        {
            Id = Guid.NewGuid(),
            RequesterId = targetId,
            TargetUserId = requesterId,
            Status = RoommateRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        
        context.RoommateRequests.AddRange(request, inverseRequest);
        await context.SaveChangesAsync();

        var handler = new RejectRequestHandler(context, CreateHttpContextAccessor(adminId));

        // Act
        var result = await handler.Handle(new RejectRequestRequest(request.Id), CancellationToken.None);

        // Assert
        result.Message.Should().Contain("rejected");
        
        var updatedRequest = await context.RoommateRequests.FindAsync(request.Id);
        updatedRequest!.Status.Should().Be(RoommateRequestStatus.Rejected);
        updatedRequest.ProcessedByAdminId.Should().Be(adminId);
        
        var updatedInverse = await context.RoommateRequests.FindAsync(inverseRequest.Id);
        updatedInverse!.Status.Should().Be(RoommateRequestStatus.Rejected);
        updatedInverse.ProcessedByAdminId.Should().Be(adminId);
    }

    #endregion

    #region DeleteRelationshipHandler Tests

    [Fact]
    public async Task Given_RelationshipNotFound_When_DeleteCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var handler = new DeleteRelationshipHandler(context);

        // Act
        Func<Task> act = () => handler.Handle(new DeleteRelationshipRequest(Guid.NewGuid()), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Given_ValidRelationship_When_DeleteCalled_Then_DeactivatesRelationship()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var user1 = new Profile { Id = Guid.NewGuid(), FullName = "User1", Email = "u1@test.com", Age=20, Gender="M", CreatedAt=DateTime.UtcNow, PasswordHash="hash" };
        var user2 = new Profile { Id = Guid.NewGuid(), FullName = "User2", Email = "u2@test.com", Age=20, Gender="M", CreatedAt=DateTime.UtcNow, PasswordHash="hash" };
        
        var relationship = new RoommateRelationship
        {
            Id = Guid.NewGuid(),
            User1Id = user1.Id,
            User2Id = user2.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ApprovedByAdminId = Guid.NewGuid()
        };
        
        context.Profiles.AddRange(user1, user2);
        context.RoommateRelationships.Add(relationship);
        await context.SaveChangesAsync();

        var handler = new DeleteRelationshipHandler(context);

        // Act
        var result = await handler.Handle(new DeleteRelationshipRequest(relationship.Id), CancellationToken.None);

        // Assert
        result.Message.Should().Contain("deactivated");
        
        var updatedRel = await context.RoommateRelationships.FindAsync(relationship.Id);
        updatedRel!.IsActive.Should().BeFalse();
    }

    #endregion
}
