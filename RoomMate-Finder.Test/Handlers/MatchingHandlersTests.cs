using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Matching.CalculateCompatibility.Services;
using RoomMate_Finder.Features.Matching.GetMatches;
using RoomMate_Finder.Features.Matching.GetUserMatches;
using RoomMate_Finder.Features.Matching.LikeProfile;
using RoomMate_Finder.Features.Matching.PassProfile;
using RoomMate_Finder.Test.Helpers;

namespace RoomMate_Finder.Test.Handlers;

#region Like Profile Handler Tests

public class LikeProfileHandlerTests
{
    private static Profile CreateTestProfile(Guid? id = null)
    {
        return new Profile
        {
            Id = id ?? Guid.NewGuid(),
            Email = $"test{Guid.NewGuid():N}@test.com",
            PasswordHash = "hashed",
            FullName = "Test User",
            Age = 25,
            Gender = "Male",
            University = "Test University",
            Bio = "Test bio",
            Lifestyle = "quiet",
            Interests = "music",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Given_UserLikesThemselves_When_HandleIsCalled_Then_ReturnsFalse()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var handler = new LikeProfileHandler(context);
        var userId = Guid.NewGuid();
        var request = new LikeProfileRequest(userId, userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot like yourself");
    }

    [Fact]
    public async Task Given_UsersDoNotExist_When_HandleIsCalled_Then_ReturnsFalse()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var handler = new LikeProfileHandler(context);
        var request = new LikeProfileRequest(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task Given_ValidUsers_When_HandleIsCalled_Then_LikeIsCreated()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user1 = CreateTestProfile();
        var user2 = CreateTestProfile();
        context.Profiles.AddRange(user1, user2);
        await context.SaveChangesAsync();

        var handler = new LikeProfileHandler(context);
        var request = new LikeProfileRequest(user1.Id, user2.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("liked successfully");
        result.IsMatch.Should().BeFalse();

        var action = await context.UserActions.FirstOrDefaultAsync(ua => 
            ua.UserId == user1.Id && ua.TargetUserId == user2.Id);
        action.Should().NotBeNull();
        action!.ActionType.Should().Be(ActionType.Like);
    }

    [Fact]
    public async Task Given_MutualLike_When_HandleIsCalled_Then_MatchIsCreated()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user1 = CreateTestProfile();
        var user2 = CreateTestProfile();
        context.Profiles.AddRange(user1, user2);
        
        // User2 already liked User1
        context.UserActions.Add(new UserAction
        {
            Id = Guid.NewGuid(),
            UserId = user2.Id,
            TargetUserId = user1.Id,
            ActionType = ActionType.Like,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var handler = new LikeProfileHandler(context);
        var request = new LikeProfileRequest(user1.Id, user2.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.IsMatch.Should().BeTrue();
        result.Message.Should().Contain("match");
        result.MatchId.Should().NotBeNull();

        // Verify match was created
        var match = await context.Matches.FirstOrDefaultAsync();
        match.Should().NotBeNull();
    }

    [Fact]
    public async Task Given_AlreadyLikedProfile_When_HandleIsCalled_Then_ReturnsFalse()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user1 = CreateTestProfile();
        var user2 = CreateTestProfile();
        context.Profiles.AddRange(user1, user2);
        context.UserActions.Add(new UserAction
        {
            Id = Guid.NewGuid(),
            UserId = user1.Id,
            TargetUserId = user2.Id,
            ActionType = ActionType.Like,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var handler = new LikeProfileHandler(context);
        var request = new LikeProfileRequest(user1.Id, user2.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already");
    }
}

#endregion

#region Pass Profile Handler Tests

public class PassProfileHandlerTests
{
    private static Profile CreateTestProfile(Guid? id = null)
    {
        return new Profile
        {
            Id = id ?? Guid.NewGuid(),
            Email = $"test{Guid.NewGuid():N}@test.com",
            PasswordHash = "hashed",
            FullName = "Test User",
            Age = 25,
            Gender = "Male",
            University = "Test University",
            Bio = "Test bio",
            Lifestyle = "quiet",
            Interests = "music",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Given_UserPassesThemselves_When_HandleIsCalled_Then_ReturnsFalse()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var handler = new PassProfileHandler(context);
        var userId = Guid.NewGuid();
        var request = new PassProfileRequest(userId, userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot pass yourself");
    }

    [Fact]
    public async Task Given_UsersDoNotExist_When_HandleIsCalled_Then_ReturnsFalse()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var handler = new PassProfileHandler(context);
        var request = new PassProfileRequest(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task Given_ValidUsers_When_HandleIsCalled_Then_PassIsCreated()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user1 = CreateTestProfile();
        var user2 = CreateTestProfile();
        context.Profiles.AddRange(user1, user2);
        await context.SaveChangesAsync();

        var handler = new PassProfileHandler(context);
        var request = new PassProfileRequest(user1.Id, user2.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("passed successfully");

        var action = await context.UserActions.FirstOrDefaultAsync(ua => 
            ua.UserId == user1.Id && ua.TargetUserId == user2.Id);
        action.Should().NotBeNull();
        action!.ActionType.Should().Be(ActionType.Pass);
    }

    [Fact]
    public async Task Given_AlreadyPassedProfile_When_HandleIsCalled_Then_ReturnsFalse()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user1 = CreateTestProfile();
        var user2 = CreateTestProfile();
        context.Profiles.AddRange(user1, user2);
        context.UserActions.Add(new UserAction
        {
            Id = Guid.NewGuid(),
            UserId = user1.Id,
            TargetUserId = user2.Id,
            ActionType = ActionType.Pass,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var handler = new PassProfileHandler(context);
        var request = new PassProfileRequest(user1.Id, user2.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already");
    }
}

#endregion

#region Get Matches Handler Tests

public class GetMatchesHandlerTests
{
    private readonly Mock<ICompatibilityCalculatorService> _mockCompatibilityService = new();

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
            University = "Test University",
            Bio = "Test bio",
            Lifestyle = "quiet",
            Interests = "music",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Given_UserDoesNotExist_When_HandleIsCalled_Then_ThrowsArgumentException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var handler = new GetMatchesHandler(context, _mockCompatibilityService.Object);
        var request = new GetMatchesRequest(Guid.NewGuid());

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Given_NoOtherProfiles_When_HandleIsCalled_Then_ReturnsEmptyList()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user = CreateTestProfile();
        context.Profiles.Add(user);
        await context.SaveChangesAsync();

        var handler = new GetMatchesHandler(context, _mockCompatibilityService.Object);
        var request = new GetMatchesRequest(user.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_OtherProfilesExist_When_HandleIsCalled_Then_ReturnsMatchesOrderedByCompatibility()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUser = CreateTestProfile(name: "Current User");
        var user1 = CreateTestProfile(name: "User 1");
        var user2 = CreateTestProfile(name: "User 2");
        context.Profiles.AddRange(currentUser, user1, user2);
        await context.SaveChangesAsync();

        _mockCompatibilityService
            .Setup(s => s.CalculateCompatibility(It.IsAny<Profile>(), It.Is<Profile>(p => p.FullName == "User 1")))
            .Returns(new CompatibilityResult(80, 80, 80, 80, 80, 80, "Very Good Match"));
        _mockCompatibilityService
            .Setup(s => s.CalculateCompatibility(It.IsAny<Profile>(), It.Is<Profile>(p => p.FullName == "User 2")))
            .Returns(new CompatibilityResult(90, 90, 90, 90, 90, 90, "Excellent Match"));

        var handler = new GetMatchesHandler(context, _mockCompatibilityService.Object);
        var request = new GetMatchesRequest(currentUser.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].FullName.Should().Be("User 2"); // Higher compatibility first
        result[1].FullName.Should().Be("User 1");
    }

    [Fact]
    public async Task Given_UserAlreadyActedOnProfile_When_HandleIsCalled_Then_ExcludesActedProfiles()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var currentUser = CreateTestProfile(name: "Current User");
        var likedUser = CreateTestProfile(name: "Liked User");
        var newUser = CreateTestProfile(name: "New User");
        context.Profiles.AddRange(currentUser, likedUser, newUser);
        context.UserActions.Add(new UserAction
        {
            Id = Guid.NewGuid(),
            UserId = currentUser.Id,
            TargetUserId = likedUser.Id,
            ActionType = ActionType.Like,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        _mockCompatibilityService
            .Setup(s => s.CalculateCompatibility(It.IsAny<Profile>(), It.IsAny<Profile>()))
            .Returns(new CompatibilityResult(75, 75, 75, 75, 75, 75, "Good Match"));

        var handler = new GetMatchesHandler(context, _mockCompatibilityService.Object);
        var request = new GetMatchesRequest(currentUser.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].FullName.Should().Be("New User");
    }
}

#endregion

#region Get User Matches Handler Tests

public class GetUserMatchesHandlerTests
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
            University = "Test University",
            Bio = "Test bio",
            Lifestyle = "quiet",
            Interests = "music",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Given_UserHasNoMatches_When_HandleIsCalled_Then_ReturnsEmptyList()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user = CreateTestProfile();
        context.Profiles.Add(user);
        await context.SaveChangesAsync();

        var handler = new GetUserMatchesHandler(context);
        var request = new GetUserMatchesRequest(user.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_UserHasMatches_When_HandleIsCalled_Then_ReturnsActiveMatches()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user1 = CreateTestProfile(name: "User 1");
        var user2 = CreateTestProfile(name: "User 2");
        var user3 = CreateTestProfile(name: "User 3");
        context.Profiles.AddRange(user1, user2, user3);

        // Create Match between user1 and user2
        var user1Id = user1.Id < user2.Id ? user1.Id : user2.Id;
        var user2Id = user1.Id < user2.Id ? user2.Id : user1.Id;
        context.Matches.Add(new RoomMate_Finder.Entities.Match
        {
            Id = Guid.NewGuid(),
            User1Id = user1Id,
            User2Id = user2Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var handler = new GetUserMatchesHandler(context);
        var request = new GetUserMatchesRequest(user1.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].FullName.Should().Be("User 2");
    }
}

#endregion
