using FluentAssertions;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Roommates.GetUserRoommate;
using RoomMate_Finder.Test.Helpers;

namespace RoomMate_Finder.Test.Handlers;

public class GetUserRoommateHandlerTests
{
    private static Profile CreateTestProfile(Guid? id = null, string name = "Test User", string email = null!)
    {
        return new Profile
        {
            Id = id ?? Guid.NewGuid(),
            Email = email ?? $"test{Guid.NewGuid():N}@test.com",
            PasswordHash = "hashed",
            FullName = name,
            Age = 25,
            Gender = "Male",
            University = "Test University",
            Bio = "Bio",
            Lifestyle = "quiet",
            Interests = "music",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Given_NoRelationship_When_HandleIsCalled_Then_ReturnsNull()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user = CreateTestProfile(name: "User");
        context.Profiles.Add(user);
        await context.SaveChangesAsync();

        var handler = new GetUserRoommateHandler(context);
        var request = new GetUserRoommateRequest(user.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_ActiveRelationshipAsUser1_When_HandleIsCalled_Then_ReturnsRoommateInfo()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user1 = CreateTestProfile(name: "User 1", email: "user1@test.com");
        var user2 = CreateTestProfile(name: "User 2", email: "user2@test.com");
        context.Profiles.AddRange(user1, user2);

        var relationship = new RoommateRelationship
        {
            Id = Guid.NewGuid(),
            User1Id = user1.Id,
            User1 = user1,
            User2Id = user2.Id,
            User2 = user2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.RoommateRelationships.Add(relationship);
        await context.SaveChangesAsync();

        var handler = new GetUserRoommateHandler(context);
        var request = new GetUserRoommateRequest(user1.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.RoommateId.Should().Be(user2.Id);
        result.RoommateName.Should().Be("User 2");
        result.RoommateEmail.Should().Be("user2@test.com");
        result.RelationshipId.Should().Be(relationship.Id);
    }

    [Fact]
    public async Task Given_ActiveRelationshipAsUser2_When_HandleIsCalled_Then_ReturnsRoommateInfo()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user1 = CreateTestProfile(name: "User 1", email: "user1@test.com");
        var user2 = CreateTestProfile(name: "User 2", email: "user2@test.com");
        context.Profiles.AddRange(user1, user2);

        var relationship = new RoommateRelationship
        {
            Id = Guid.NewGuid(),
            User1Id = user1.Id,
            User1 = user1,
            User2Id = user2.Id,
            User2 = user2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.RoommateRelationships.Add(relationship);
        await context.SaveChangesAsync();

        var handler = new GetUserRoommateHandler(context);
        var request = new GetUserRoommateRequest(user2.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.RoommateId.Should().Be(user1.Id);
        result.RoommateName.Should().Be("User 1");
        result.RoommateEmail.Should().Be("user1@test.com");
    }

    [Fact]
    public async Task Given_InactiveRelationship_When_HandleIsCalled_Then_ReturnsNull()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user1 = CreateTestProfile(name: "User 1");
        var user2 = CreateTestProfile(name: "User 2");
        context.Profiles.AddRange(user1, user2);

        var relationship = new RoommateRelationship
        {
            Id = Guid.NewGuid(),
            User1Id = user1.Id,
            User1 = user1,
            User2Id = user2.Id,
            User2 = user2,
            IsActive = false, // Inactive relationship
            CreatedAt = DateTime.UtcNow
        };
        context.RoommateRelationships.Add(relationship);
        await context.SaveChangesAsync();

        var handler = new GetUserRoommateHandler(context);
        var request = new GetUserRoommateRequest(user1.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_UserNotInAnyRelationship_When_HandleIsCalled_Then_ReturnsNull()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user1 = CreateTestProfile(name: "User 1");
        var user2 = CreateTestProfile(name: "User 2");
        var user3 = CreateTestProfile(name: "User 3");
        context.Profiles.AddRange(user1, user2, user3);

        // Relationship between user1 and user2, but not user3
        var relationship = new RoommateRelationship
        {
            Id = Guid.NewGuid(),
            User1Id = user1.Id,
            User1 = user1,
            User2Id = user2.Id,
            User2 = user2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.RoommateRelationships.Add(relationship);
        await context.SaveChangesAsync();

        var handler = new GetUserRoommateHandler(context);
        var request = new GetUserRoommateRequest(user3.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Given_RelationshipWithProfilePicture_When_HandleIsCalled_Then_ReturnsProfilePicturePath()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user1 = CreateTestProfile(name: "User 1");
        var user2 = CreateTestProfile(name: "User 2");
        user2.ProfilePicturePath = "/images/profile.jpg";
        context.Profiles.AddRange(user1, user2);

        var relationship = new RoommateRelationship
        {
            Id = Guid.NewGuid(),
            User1Id = user1.Id,
            User1 = user1,
            User2Id = user2.Id,
            User2 = user2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.RoommateRelationships.Add(relationship);
        await context.SaveChangesAsync();

        var handler = new GetUserRoommateHandler(context);
        var request = new GetUserRoommateRequest(user1.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.ProfilePicturePath.Should().Be("/images/profile.jpg");
    }

    [Fact]
    public async Task Given_RelationshipWithAge_When_HandleIsCalled_Then_ReturnsCorrectAge()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user1 = CreateTestProfile(name: "User 1");
        var user2 = CreateTestProfile(name: "User 2");
        user2.Age = 30;
        context.Profiles.AddRange(user1, user2);

        var relationship = new RoommateRelationship
        {
            Id = Guid.NewGuid(),
            User1Id = user1.Id,
            User1 = user1,
            User2Id = user2.Id,
            User2 = user2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.RoommateRelationships.Add(relationship);
        await context.SaveChangesAsync();

        var handler = new GetUserRoommateHandler(context);
        var request = new GetUserRoommateRequest(user1.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Age.Should().Be(30);
    }
}

