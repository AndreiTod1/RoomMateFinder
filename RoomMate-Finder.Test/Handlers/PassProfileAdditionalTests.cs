using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Matching.PassProfile;
using RoomMate_Finder.Test.Helpers;
using Xunit;

namespace RoomMate_Finder.Test.Handlers;

public class PassProfileAdditionalTests : IDisposable
{
    [Fact]
    public async Task Given_PassOlderUser_When_HandleIsCalled_Then_CreatesPassAction()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var userId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        var user = new Profile
        {
            Id = userId,
            Email = "younger@example.com",
            PasswordHash = "hashedpass",
            FullName = "Younger User",
            Age = 20,
            Gender = "F",
            University = "Young University",
            Bio = "Young and energetic",
            Lifestyle = "Active",
            Interests = "Parties, Clubbing",
            CreatedAt = DateTime.UtcNow
        };

        var targetUser = new Profile
        {
            Id = targetUserId,
            Email = "older@example.com",
            PasswordHash = "hashedpass2",
            FullName = "Older User",
            Age = 45,
            Gender = "M",
            University = "Mature University",
            Bio = "Experienced professional",
            Lifestyle = "Calm",
            Interests = "Reading, Classical Music",
            CreatedAt = DateTime.UtcNow
        };

        context.Profiles.AddRange(user, targetUser);
        await context.SaveChangesAsync();

        var handler = new PassProfileHandler(context);
        var request = new PassProfileRequest(userId, targetUserId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Profile passed successfully");
        
        var userAction = context.UserActions.FirstOrDefault(ua => ua.UserId == userId && ua.TargetUserId == targetUserId);
        userAction.Should().NotBeNull();
        userAction!.ActionType.Should().Be(ActionType.Pass);
    }

    [Fact]
    public async Task Given_PassDifferentLifestyle_When_HandleIsCalled_Then_WorksCorrectly()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var userId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        var user = new Profile
        {
            Id = userId,
            Email = "party@example.com",
            PasswordHash = "hashedpass",
            FullName = "Party Person",
            Age = 22,
            Gender = "M",
            University = "Party University",
            Bio = "Love to party",
            Lifestyle = "Social",
            Interests = "Nightlife, Dancing, Music",
            CreatedAt = DateTime.UtcNow
        };

        var targetUser = new Profile
        {
            Id = targetUserId,
            Email = "quiet@example.com",
            PasswordHash = "hashedpass2",
            FullName = "Quiet Person",
            Age = 24,
            Gender = "F",
            University = "Quiet University",
            Bio = "Prefer peace and quiet",
            Lifestyle = "Introverted",
            Interests = "Books, Meditation, Tea",
            CreatedAt = DateTime.UtcNow
        };

        context.Profiles.AddRange(user, targetUser);
        await context.SaveChangesAsync();

        var handler = new PassProfileHandler(context);
        var request = new PassProfileRequest(userId, targetUserId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Given_PassDifferentUniversity_When_HandleIsCalled_Then_CreatesAction()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var userId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        var user = new Profile
        {
            Id = userId,
            Email = "local@example.com",
            PasswordHash = "hashedpass",
            FullName = "Local Student",
            Age = 21,
            Gender = "F",
            University = "Local Community College",
            Bio = "Local student",
            Lifestyle = "Simple",
            Interests = "Local activities",
            CreatedAt = DateTime.UtcNow
        };

        var targetUser = new Profile
        {
            Id = targetUserId,
            Email = "ivy@example.com",
            PasswordHash = "hashedpass2",
            FullName = "Ivy League Student",
            Age = 22,
            Gender = "M",
            University = "Yale University",
            Bio = "Privileged background",
            Lifestyle = "Elite",
            Interests = "Exclusive events, Networking",
            CreatedAt = DateTime.UtcNow
        };

        context.Profiles.AddRange(user, targetUser);
        await context.SaveChangesAsync();

        var handler = new PassProfileHandler(context);
        var request = new PassProfileRequest(userId, targetUserId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Profile passed successfully");
    }

    [Fact]
    public async Task Given_PassIncompatibleInterests_When_HandleIsCalled_Then_WorksCorrectly()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var userId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        var user = new Profile
        {
            Id = userId,
            Email = "outdoor@example.com",
            PasswordHash = "hashedpass",
            FullName = "Outdoor Enthusiast",
            Age = 26,
            Gender = "M",
            University = "Nature University",
            Bio = "Love the outdoors",
            Lifestyle = "Adventurous",
            Interests = "Hiking, Camping, Rock Climbing, Fishing",
            CreatedAt = DateTime.UtcNow
        };

        var targetUser = new Profile
        {
            Id = targetUserId,
            Email = "indoor@example.com",
            PasswordHash = "hashedpass2",
            FullName = "Indoor Person",
            Age = 27,
            Gender = "F",
            University = "Tech University",
            Bio = "Prefer indoor activities",
            Lifestyle = "Sedentary",
            Interests = "Gaming, Coding, Netflix, Junk Food",
            CreatedAt = DateTime.UtcNow
        };

        context.Profiles.AddRange(user, targetUser);
        await context.SaveChangesAsync();

        var handler = new PassProfileHandler(context);
        var request = new PassProfileRequest(userId, targetUserId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        
        var userAction = context.UserActions.FirstOrDefault(ua => ua.UserId == userId && ua.TargetUserId == targetUserId);
        userAction.Should().NotBeNull();
        userAction!.ActionType.Should().Be(ActionType.Pass);
        userAction.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    public void Dispose()
    {
        // Clean up resources if needed
    }
}
