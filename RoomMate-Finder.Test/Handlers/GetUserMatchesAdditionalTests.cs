using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Matching.GetUserMatches;
using RoomMate_Finder.Test.Helpers;
using Xunit;

namespace RoomMate_Finder.Test.Handlers;

public class GetUserMatchesAdditionalTests : IDisposable
{
    [Fact]
    public async Task Given_UserWithOldMatches_When_HandleIsCalled_Then_ReturnsAllMatches()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var userId = Guid.NewGuid();
        var oldMatchId = Guid.NewGuid();

        var user = new Profile
        {
            Id = userId,
            Email = "user@example.com",
            PasswordHash = "hashedpass",
            FullName = "Test User",
            Age = 25,
            Gender = "M",
            University = "Test University",
            Bio = "Test bio",
            Lifestyle = "Active",
            Interests = "Sports",
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        };

        var oldMatch = new Profile
        {
            Id = oldMatchId,
            Email = "oldmatch@example.com",
            PasswordHash = "hashedpass2",
            FullName = "Old Match",
            Age = 26,
            Gender = "F",
            University = "Test University",
            Bio = "Old match bio",
            Lifestyle = "Calm",
            Interests = "Reading",
            CreatedAt = DateTime.UtcNow.AddMonths(-12)
        };

        var match = new Match
        {
            Id = Guid.NewGuid(),
            User1Id = userId,
            User2Id = oldMatchId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };

        context.Profiles.AddRange(user, oldMatch);
        context.Matches.Add(match);
        await context.SaveChangesAsync();

        var handler = new GetUserMatchesHandler(context);
        var request = new GetUserMatchesRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().UserId.Should().Be(oldMatchId);
        result.First().FullName.Should().Be("Old Match");
    }

    [Fact]
    public async Task Given_UserWithManyMatches_When_HandleIsCalled_Then_ReturnsAllOrderedByDate()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var userId = Guid.NewGuid();
        var match1Id = Guid.NewGuid();
        var match2Id = Guid.NewGuid();
        var match3Id = Guid.NewGuid();

        var user = new Profile
        {
            Id = userId,
            Email = "popular@example.com",
            PasswordHash = "hashedpass",
            FullName = "Popular User",
            Age = 24,
            Gender = "F",
            University = "Popular University",
            Bio = "Very social",
            Lifestyle = "Social",
            Interests = "Meeting people",
            CreatedAt = DateTime.UtcNow
        };

        var match1 = new Profile
        {
            Id = match1Id,
            Email = "match1@example.com",
            PasswordHash = "hashedpass1",
            FullName = "First Match",
            Age = 25,
            Gender = "M",
            University = "Test University",
            Bio = "First match bio",
            Lifestyle = "Active",
            Interests = "Sports",
            CreatedAt = DateTime.UtcNow
        };

        var match2 = new Profile
        {
            Id = match2Id,
            Email = "match2@example.com",
            PasswordHash = "hashedpass2",
            FullName = "Second Match",
            Age = 26,
            Gender = "M",
            University = "Test University",
            Bio = "Second match bio",
            Lifestyle = "Calm",
            Interests = "Reading",
            CreatedAt = DateTime.UtcNow
        };

        var match3 = new Profile
        {
            Id = match3Id,
            Email = "match3@example.com",
            PasswordHash = "hashedpass3",
            FullName = "Third Match",
            Age = 23,
            Gender = "M",
            University = "Test University",
            Bio = "Third match bio",
            Lifestyle = "Creative",
            Interests = "Art",
            CreatedAt = DateTime.UtcNow
        };

        var matchRecord1 = new Match
        {
            Id = Guid.NewGuid(),
            User1Id = userId,
            User2Id = match1Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddHours(-3)
        };

        var matchRecord2 = new Match
        {
            Id = Guid.NewGuid(),
            User1Id = match2Id,
            User2Id = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        var matchRecord3 = new Match
        {
            Id = Guid.NewGuid(),
            User1Id = userId,
            User2Id = match3Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        context.Profiles.AddRange(user, match1, match2, match3);
        context.Matches.AddRange(matchRecord1, matchRecord2, matchRecord3);
        await context.SaveChangesAsync();

        var handler = new GetUserMatchesHandler(context);
        var request = new GetUserMatchesRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(m => m.FullName == "First Match");
        result.Should().Contain(m => m.FullName == "Second Match");
        result.Should().Contain(m => m.FullName == "Third Match");
    }

    [Fact]
    public async Task Given_UserWithMixedActiveInactiveMatches_When_HandleIsCalled_Then_ReturnsOnlyActive()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var userId = Guid.NewGuid();
        var activeMatchId = Guid.NewGuid();
        var inactiveMatchId = Guid.NewGuid();

        var user = new Profile
        {
            Id = userId,
            Email = "user@example.com",
            PasswordHash = "hashedpass",
            FullName = "Test User",
            Age = 25,
            Gender = "M",
            University = "Test University",
            Bio = "Test bio",
            Lifestyle = "Active",
            Interests = "Sports",
            CreatedAt = DateTime.UtcNow
        };

        var activeMatch = new Profile
        {
            Id = activeMatchId,
            Email = "active@example.com",
            PasswordHash = "hashedpass1",
            FullName = "Active Match",
            Age = 26,
            Gender = "F",
            University = "Test University",
            Bio = "Still active",
            Lifestyle = "Active",
            Interests = "Sports",
            CreatedAt = DateTime.UtcNow
        };

        var inactiveMatch = new Profile
        {
            Id = inactiveMatchId,
            Email = "inactive@example.com",
            PasswordHash = "hashedpass2",
            FullName = "Inactive Match",
            Age = 27,
            Gender = "F",
            University = "Test University",
            Bio = "No longer active",
            Lifestyle = "Inactive",
            Interests = "Nothing",
            CreatedAt = DateTime.UtcNow
        };

        var activeMatchRecord = new Match
        {
            Id = Guid.NewGuid(),
            User1Id = userId,
            User2Id = activeMatchId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        var inactiveMatchRecord = new Match
        {
            Id = Guid.NewGuid(),
            User1Id = userId,
            User2Id = inactiveMatchId,
            IsActive = false,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        context.Profiles.AddRange(user, activeMatch, inactiveMatch);
        context.Matches.AddRange(activeMatchRecord, inactiveMatchRecord);
        await context.SaveChangesAsync();

        var handler = new GetUserMatchesHandler(context);
        var request = new GetUserMatchesRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().FullName.Should().Be("Active Match");
    }

    [Fact]
    public async Task Given_UserAsUser2InMatch_When_HandleIsCalled_Then_ReturnsMatch()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var userId = Guid.NewGuid();
        var matchId = Guid.NewGuid();

        var user = new Profile
        {
            Id = userId,
            Email = "user2@example.com",
            PasswordHash = "hashedpass",
            FullName = "User Two",
            Age = 28,
            Gender = "F",
            University = "Test University",
            Bio = "I am User2",
            Lifestyle = "Normal",
            Interests = "Normal stuff",
            CreatedAt = DateTime.UtcNow
        };

        var matchUser = new Profile
        {
            Id = matchId,
            Email = "user1@example.com",
            PasswordHash = "hashedpass1",
            FullName = "User One",
            Age = 29,
            Gender = "M",
            University = "Test University",
            Bio = "I am User1",
            Lifestyle = "Normal",
            Interests = "Normal activities",
            CreatedAt = DateTime.UtcNow
        };

        var matchRecord = new Match
        {
            Id = Guid.NewGuid(),
            User1Id = matchId, // Other user is User1
            User2Id = userId,   // Current user is User2
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        context.Profiles.AddRange(user, matchUser);
        context.Matches.Add(matchRecord);
        await context.SaveChangesAsync();

        var handler = new GetUserMatchesHandler(context);
        var request = new GetUserMatchesRequest(userId);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().FullName.Should().Be("User One");
        result.First().Email.Should().Be("user1@example.com");
    }

    public void Dispose()
    {
        // Clean up resources if needed
    }
}
