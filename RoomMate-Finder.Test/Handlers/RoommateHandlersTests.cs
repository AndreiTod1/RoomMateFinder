using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Roommates.GetPendingRequests;
using RoomMate_Finder.Features.Roommates.GetRelationships;
using RoomMate_Finder.Features.Roommates.RejectRequest;
using RoomMate_Finder.Features.Roommates.DeleteRelationship;
using RoomMate_Finder.Test.Helpers;
using System.Security.Claims;

namespace RoomMate_Finder.Test.Handlers;

#region Get Pending Requests Handler Tests

public class GetPendingRequestsHandlerTests
{
    private static Profile CreateTestProfile(Guid? id = null, string name = "Test User")
    {
        return new Profile
        {
            Id = id ?? Guid.NewGuid(),
            Email = $"test{Guid.NewGuid():N}@test.com",
            PasswordHash = "hashed",
            FullName = name,
            Age = 25,
            Gender = "Male",
            University = "Test Uni",
            Bio = "Bio",
            Lifestyle = "quiet",
            Interests = "music",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Given_NoPendingRequests_When_HandleIsCalled_Then_ReturnsEmptyList()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var handler = new GetPendingRequestsHandler(context);
        var request = new GetPendingRequestsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_MutuallyConfirmedRequests_When_HandleIsCalled_Then_ReturnsPendingRequests()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user1 = CreateTestProfile(name: "User 1");
        var user2 = CreateTestProfile(name: "User 2");
        context.Profiles.AddRange(user1, user2);

        var roommateRequest = new RoommateRequest
        {
            Id = Guid.NewGuid(),
            RequesterId = user1.Id,
            Requester = user1,
            TargetUserId = user2.Id,
            TargetUser = user2,
            Status = RoommateRequestStatus.MutuallyConfirmed,
            CreatedAt = DateTime.UtcNow
        };
        context.RoommateRequests.Add(roommateRequest);
        await context.SaveChangesAsync();

        var handler = new GetPendingRequestsHandler(context);
        var request = new GetPendingRequestsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].RequesterName.Should().Be("User 1");
        result[0].TargetUserName.Should().Be("User 2");
    }

    [Fact]
    public async Task Given_OnlyPendingStatusRequests_When_HandleIsCalled_Then_ReturnsEmptyList()
    {
        // Arrange - Only Pending status, not MutuallyConfirmed
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user1 = CreateTestProfile(name: "User 1");
        var user2 = CreateTestProfile(name: "User 2");
        context.Profiles.AddRange(user1, user2);

        var roommateRequest = new RoommateRequest
        {
            Id = Guid.NewGuid(),
            RequesterId = user1.Id,
            Requester = user1,
            TargetUserId = user2.Id,
            TargetUser = user2,
            Status = RoommateRequestStatus.Pending, // Not MutuallyConfirmed
            CreatedAt = DateTime.UtcNow
        };
        context.RoommateRequests.Add(roommateRequest);
        await context.SaveChangesAsync();

        var handler = new GetPendingRequestsHandler(context);
        var request = new GetPendingRequestsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert - Should return empty since only MutuallyConfirmed are shown
        result.Should().BeEmpty();
    }
}

#endregion

#region Get Relationships Handler Tests

public class GetRelationshipsHandlerTests
{
    private static Profile CreateTestProfile(Guid? id = null, string name = "Test User")
    {
        return new Profile
        {
            Id = id ?? Guid.NewGuid(),
            Email = $"test{Guid.NewGuid():N}@test.com",
            PasswordHash = "hashed",
            FullName = name,
            Age = 25,
            Gender = "Male",
            University = "Test Uni",
            Bio = "Bio",
            Lifestyle = "quiet",
            Interests = "music",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Given_NoRelationships_When_HandleIsCalled_Then_ReturnsEmptyList()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var handler = new GetRelationshipsHandler(context);
        var request = new GetRelationshipsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_ActiveRelationships_When_HandleIsCalled_Then_ReturnsRelationships()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user1 = CreateTestProfile(name: "User 1");
        var user2 = CreateTestProfile(name: "User 2");
        var admin = CreateTestProfile(name: "Admin");
        admin.Role = "Admin";
        context.Profiles.AddRange(user1, user2, admin);

        var relationship = new RoommateRelationship
        {
            Id = Guid.NewGuid(),
            User1Id = user1.Id,
            User1 = user1,
            User2Id = user2.Id,
            User2 = user2,
            ApprovedByAdminId = admin.Id,
            ApprovedByAdmin = admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.RoommateRelationships.Add(relationship);
        await context.SaveChangesAsync();

        var handler = new GetRelationshipsHandler(context);
        var request = new GetRelationshipsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].User1Name.Should().Be("User 1");
        result[0].User2Name.Should().Be("User 2");
    }
}

#endregion

#region Reject Request Handler Tests

public class RejectRequestHandlerTests
{
    private static Profile CreateTestProfile(Guid? id = null, string name = "Test User", string role = "User")
    {
        return new Profile
        {
            Id = id ?? Guid.NewGuid(),
            Email = $"test{Guid.NewGuid():N}@test.com",
            PasswordHash = "hashed",
            FullName = name,
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

    private static Mock<IHttpContextAccessor> CreateMockHttpContextWithUser(Guid userId)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.User).Returns(principal);
        
        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);
        
        return mockAccessor;
    }

    [Fact]
    public async Task Given_RequestNotFound_When_HandleIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var adminId = Guid.NewGuid();
        var mockAccessor = CreateMockHttpContextWithUser(adminId);
        
        var handler = new RejectRequestHandler(context, mockAccessor.Object);
        var request = new RejectRequestRequest(Guid.NewGuid());

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Given_ValidRequest_When_HandleIsCalled_Then_RequestIsRejected()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user1 = CreateTestProfile(name: "User 1");
        var user2 = CreateTestProfile(name: "User 2");
        var admin = CreateTestProfile(name: "Admin", role: "Admin");
        context.Profiles.AddRange(user1, user2, admin);

        var roommateRequest = new RoommateRequest
        {
            Id = Guid.NewGuid(),
            RequesterId = user1.Id,
            Requester = user1,
            TargetUserId = user2.Id,
            TargetUser = user2,
            Status = RoommateRequestStatus.MutuallyConfirmed,
            CreatedAt = DateTime.UtcNow
        };
        context.RoommateRequests.Add(roommateRequest);
        await context.SaveChangesAsync();

        var mockAccessor = CreateMockHttpContextWithUser(admin.Id);
        var handler = new RejectRequestHandler(context, mockAccessor.Object);
        var request = new RejectRequestRequest(roommateRequest.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Message.Should().Contain("rejected");
        var updatedRequest = await context.RoommateRequests.FindAsync(roommateRequest.Id);
        updatedRequest!.Status.Should().Be(RoommateRequestStatus.Rejected);
    }
}

#endregion

#region Delete Relationship Handler Tests

public class DeleteRelationshipHandlerTests
{
    private static Profile CreateTestProfile(Guid? id = null, string name = "Test User", string role = "User")
    {
        return new Profile
        {
            Id = id ?? Guid.NewGuid(),
            Email = $"test{Guid.NewGuid():N}@test.com",
            PasswordHash = "hashed",
            FullName = name,
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
    public async Task Given_RelationshipNotFound_When_HandleIsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var handler = new DeleteRelationshipHandler(context);
        var request = new DeleteRelationshipRequest(Guid.NewGuid());

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Given_ValidRelationship_When_HandleIsCalled_Then_RelationshipIsDeactivated()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user1 = CreateTestProfile(name: "User 1");
        var user2 = CreateTestProfile(name: "User 2");
        var admin = CreateTestProfile(name: "Admin", role: "Admin");
        context.Profiles.AddRange(user1, user2, admin);

        var relationship = new RoommateRelationship
        {
            Id = Guid.NewGuid(),
            User1Id = user1.Id,
            User1 = user1,
            User2Id = user2.Id,
            User2 = user2,
            ApprovedByAdminId = admin.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.RoommateRelationships.Add(relationship);
        await context.SaveChangesAsync();

        var handler = new DeleteRelationshipHandler(context);
        var request = new DeleteRelationshipRequest(relationship.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Message.Should().Contain("deactivated");
        var updatedRelationship = await context.RoommateRelationships.FindAsync(relationship.Id);
        updatedRelationship!.IsActive.Should().BeFalse();
    }
}

#endregion
