using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Matching.LikeProfile;
using RoomMate_Finder.Test.Helpers;
using Xunit;

namespace RoomMate_Finder.Test.Handlers;

public class LikeProfileAdditionalTests : IDisposable
{
    [Fact]
    public async Task Given_LikeAfterLongTime_When_HandleIsCalled_Then_CreatesUserAction()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var userId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        var user = new Profile
        {
            Id = userId,
            Email = "olduser@example.com",
            PasswordHash = "hashedpass",
            FullName = "Old User",
            Age = 35,
            Gender = "M",
            University = "Old University",
            Bio = "Experienced user",
            Lifestyle = "Mature",
            Interests = "Reading, Cooking",
            CreatedAt = DateTime.UtcNow.AddYears(-2)
        };

        var targetUser = new Profile
        {
            Id = targetUserId,
            Email = "newuser@example.com",
            PasswordHash = "hashedpass2",
            FullName = "New User",
            Age = 22,
            Gender = "F",
            University = "New University",
            Bio = "Fresh graduate",
            Lifestyle = "Energetic",
            Interests = "Parties, Dancing",
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

        context.Profiles.AddRange(user, targetUser);
        await context.SaveChangesAsync();

        var handler = new LikeProfileHandler(context);
        var request = new LikeProfileRequest(userId, targetUserId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Profile liked successfully");
    }

    [Fact]
    public async Task Given_YoungUsersLiking_When_HandleIsCalled_Then_WorksCorrectly()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var userId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        var user = new Profile
        {
            Id = userId,
            Email = "young1@example.com",
            PasswordHash = "hashedpass",
            FullName = "Young User 1",
            Age = 18,
            Gender = "F",
            University = "Young University",
            Bio = "Just started college",
            Lifestyle = "Student",
            Interests = "Study, Movies",
            CreatedAt = DateTime.UtcNow
        };

        var targetUser = new Profile
        {
            Id = targetUserId,
            Email = "young2@example.com",
            PasswordHash = "hashedpass2",
            FullName = "Young User 2",
            Age = 19,
            Gender = "M",
            University = "Young University",
            Bio = "Freshman",
            Lifestyle = "Student",
            Interests = "Games, Sports",
            CreatedAt = DateTime.UtcNow
        };

        context.Profiles.AddRange(user, targetUser);
        await context.SaveChangesAsync();

        var handler = new LikeProfileHandler(context);
        var request = new LikeProfileRequest(userId, targetUserId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        var userAction = context.UserActions.FirstOrDefault(ua => ua.UserId == userId && ua.TargetUserId == targetUserId);
        userAction.Should().NotBeNull();
        userAction!.ActionType.Should().Be(ActionType.Like);
    }

    [Fact]
    public async Task Given_DifferentUniversityUsers_When_HandleIsCalled_Then_WorksCorrectly()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var userId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        var user = new Profile
        {
            Id = userId,
            Email = "harvard@example.com",
            PasswordHash = "hashedpass",
            FullName = "Harvard Student",
            Age = 24,
            Gender = "M",
            University = "Harvard University",
            Bio = "Ivy league student",
            Lifestyle = "Academic",
            Interests = "Research, Science",
            CreatedAt = DateTime.UtcNow
        };

        var targetUser = new Profile
        {
            Id = targetUserId,
            Email = "stanford@example.com",
            PasswordHash = "hashedpass2",
            FullName = "Stanford Student",
            Age = 23,
            Gender = "F",
            University = "Stanford University",
            Bio = "Tech enthusiast",
            Lifestyle = "Innovative",
            Interests = "Technology, AI",
            CreatedAt = DateTime.UtcNow
        };

        context.Profiles.AddRange(user, targetUser);
        await context.SaveChangesAsync();

        var handler = new LikeProfileHandler(context);
        var request = new LikeProfileRequest(userId, targetUserId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Given_SimilarInterests_When_HandleIsCalled_Then_CreatesLike()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var userId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        var user = new Profile
        {
            Id = userId,
            Email = "sports1@example.com",
            PasswordHash = "hashedpass",
            FullName = "Sports Fan 1",
            Age = 27,
            Gender = "M",
            University = "Sports University",
            Bio = "Love all sports",
            Lifestyle = "Athletic",
            Interests = "Football, Basketball, Tennis, Swimming",
            CreatedAt = DateTime.UtcNow
        };

        var targetUser = new Profile
        {
            Id = targetUserId,
            Email = "sports2@example.com",
            PasswordHash = "hashedpass2",
            FullName = "Sports Fan 2",
            Age = 25,
            Gender = "F",
            University = "Athletic University",
            Bio = "Fitness lover",
            Lifestyle = "Athletic",
            Interests = "Basketball, Tennis, Volleyball, Running",
            CreatedAt = DateTime.UtcNow
        };

        context.Profiles.AddRange(user, targetUser);
        await context.SaveChangesAsync();

        var handler = new LikeProfileHandler(context);
        var request = new LikeProfileRequest(userId, targetUserId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.IsMatch.Should().BeFalse(); // No mutual like yet
    }

    [Fact]
    public async Task Given_ComplexMutualLike_When_HandleIsCalled_Then_CreatesMatch()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var userId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        var user = new Profile
        {
            Id = userId,
            Email = "user1@complex.com",
            PasswordHash = "hashedpass",
            FullName = "Complex User 1",
            Age = 30,
            Gender = "M",
            University = "Complex University",
            Bio = "Complex personality",
            Lifestyle = "Balanced",
            Interests = "Multiple hobbies",
            CreatedAt = DateTime.UtcNow
        };

        var targetUser = new Profile
        {
            Id = targetUserId,
            Email = "user2@complex.com",
            PasswordHash = "hashedpass2",
            FullName = "Complex User 2",
            Age = 28,
            Gender = "F",
            University = "Complex University",
            Bio = "Interesting person",
            Lifestyle = "Balanced",
            Interests = "Various activities",
            CreatedAt = DateTime.UtcNow
        };

        // Target user already liked the main user
        var existingUserAction = new UserAction
        {
            Id = Guid.NewGuid(),
            UserId = targetUserId,
            TargetUserId = userId,
            ActionType = ActionType.Like,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        context.Profiles.AddRange(user, targetUser);
        context.UserActions.Add(existingUserAction);
        await context.SaveChangesAsync();

        var handler = new LikeProfileHandler(context);
        var request = new LikeProfileRequest(userId, targetUserId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.IsMatch.Should().BeTrue();
        result.MatchId.Should().NotBeNull();
    }

    public void Dispose()
    {
        // Clean up resources if needed
    }
}
