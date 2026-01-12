using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Roommates.GetMyRequests;
using RoomMate_Finder.Test.Helpers;
using System.Security.Claims;

namespace RoomMate_Finder.Test.Handlers;

public class RoommateReadHandlersTests
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

    #region GetMyRequestsHandler Tests

    [Fact]
    public async Task Given_NoAuth_When_GetMyRequestsIsCalled_Then_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null!); // or empty context
        
        var handler = new GetMyRequestsHandler(context, mockAccessor.Object);

        // Act
        Func<Task> act = () => handler.Handle(new GetMyRequestsRequest(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Given_NoData_When_GetMyRequestsIsCalled_Then_ReturnsEmptyLists()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var userId = Guid.NewGuid();
        var handler = new GetMyRequestsHandler(context, CreateHttpContextAccessor(userId));

        // Act
        var result = await handler.Handle(new GetMyRequestsRequest(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SentRequests.Should().BeEmpty();
        result.ReceivedRequests.Should().BeEmpty();
        result.ActiveRoommates.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_SentRequests_When_GetMyRequestsIsCalled_Then_ReturnsSentRequests()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var userId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        
        var user = new Profile { Id = userId, FullName = "Me", Email = "me@test.com", CreatedAt = DateTime.UtcNow, PasswordHash = "hash", Age=20, Gender="M" };
        var target = new Profile { Id = targetId, FullName = "Target", Email = "target@test.com", CreatedAt = DateTime.UtcNow, PasswordHash = "hash", Age=20, Gender="F" };
        
        var request = new RoommateRequest
        {
            Id = Guid.NewGuid(),
            RequesterId = userId,
            TargetUserId = targetId,
            Message = "Please be my roommate",
            Status = RoommateRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Profiles.AddRange(user, target);
        context.RoommateRequests.Add(request);
        await context.SaveChangesAsync();

        var handler = new GetMyRequestsHandler(context, CreateHttpContextAccessor(userId));

        // Act
        var result = await handler.Handle(new GetMyRequestsRequest(), CancellationToken.None);

        // Assert
        result.SentRequests.Should().HaveCount(1);
        result.SentRequests[0].OtherUserName.Should().Be("Target");
        result.SentRequests[0].Status.Should().Be("Pending");
        
        result.ReceivedRequests.Should().BeEmpty();
        result.ActiveRoommates.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_ReceivedRequests_When_GetMyRequestsIsCalled_Then_ReturnsReceivedRequests()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var userId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        
        var user = new Profile { Id = userId, FullName = "Me", Email = "me@test.com", CreatedAt = DateTime.UtcNow, PasswordHash = "hash", Age=20, Gender="M" };
        var sender = new Profile { Id = senderId, FullName = "Sender", Email = "sender@test.com", CreatedAt = DateTime.UtcNow, PasswordHash = "hash", Age=20, Gender="M" };
        
        var request = new RoommateRequest
        {
            Id = Guid.NewGuid(),
            RequesterId = senderId,
            TargetUserId = userId,
            Message = "I want to move in",
            Status = RoommateRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        
        context.Profiles.AddRange(user, sender);
        context.RoommateRequests.Add(request);
        await context.SaveChangesAsync();

        var handler = new GetMyRequestsHandler(context, CreateHttpContextAccessor(userId));

        // Act
        var result = await handler.Handle(new GetMyRequestsRequest(), CancellationToken.None);

        // Assert
        result.ReceivedRequests.Should().HaveCount(1);
        result.ReceivedRequests[0].OtherUserName.Should().Be("Sender");
        result.ReceivedRequests[0].Status.Should().Be("Pending");

        result.SentRequests.Should().BeEmpty();
        result.ActiveRoommates.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_ActiveRelationship_When_GetMyRequestsIsCalled_Then_ReturnsActiveRoommates()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var userId = Guid.NewGuid();
        var roommateId = Guid.NewGuid();
        
        var user = new Profile { Id = userId, FullName = "Me", Email = "me@test.com", CreatedAt = DateTime.UtcNow, PasswordHash = "hash", Age=20, Gender="M" };
        var roommate = new Profile { Id = roommateId, FullName = "Roommate", Email = "room@test.com", CreatedAt = DateTime.UtcNow, PasswordHash = "hash", Age=20, Gender="M" };
        
        var relationship = new RoommateRelationship
        {
            Id = Guid.NewGuid(),
            User1Id = userId,
            User2Id = roommateId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ApprovedByAdminId = Guid.NewGuid()
        };
        
        context.Profiles.AddRange(user, roommate);
        context.RoommateRelationships.Add(relationship);
        await context.SaveChangesAsync();

        var handler = new GetMyRequestsHandler(context, CreateHttpContextAccessor(userId));

        // Act
        var result = await handler.Handle(new GetMyRequestsRequest(), CancellationToken.None);

        // Assert
        result.ActiveRoommates.Should().HaveCount(1);
        result.ActiveRoommates[0].RoommateName.Should().Be("Roommate"); // Assuming DTO property name
        
        result.SentRequests.Should().BeEmpty();
        result.ReceivedRequests.Should().BeEmpty();
    }

    #endregion
}
