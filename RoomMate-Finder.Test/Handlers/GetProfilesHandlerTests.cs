using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Profiles.GetProfiles;
using RoomMate_Finder.Test.Helpers;
using Xunit;

namespace RoomMate_Finder.Test.Handlers;

public class GetProfilesHandlerTests
{
    [Fact]
    public async Task Given_EmptyDatabase_When_HandleIsCalled_Then_EmptyListIsReturned()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var handler = new GetProfilesHandler(context);
        var request = new GetProfilesRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_DatabaseWithProfiles_When_HandleIsCalled_Then_AllProfilesAreReturned()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var profile1 = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "user1@example.com",
            PasswordHash = "hashedpass1",
            FullName = "User One",
            Age = 25,
            Gender = "M",
            University = "University One",
            Bio = "Bio one",
            Lifestyle = "Active",
            Interests = "Sports, Music",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var profile2 = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "user2@example.com",
            PasswordHash = "hashedpass2",
            FullName = "User Two",
            Age = 30,
            Gender = "F",
            University = "University Two",
            Bio = "Bio two",
            Lifestyle = "Calm",
            Interests = "Reading, Art",
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        var profile3 = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "user3@example.com",
            PasswordHash = "hashedpass3",
            FullName = "User Three",
            Age = 22,
            Gender = "M",
            University = "University Three",
            Bio = "Bio three",
            Lifestyle = "Social",
            Interests = "Gaming, Coding",
            CreatedAt = DateTime.UtcNow
        };

        context.Profiles.AddRange(profile1, profile2, profile3);
        await context.SaveChangesAsync();

        var handler = new GetProfilesHandler(context);
        var request = new GetProfilesRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        
        // Verify all profiles are returned with correct data
        var returnedProfile1 = result.First(p => p.Id == profile1.Id);
        returnedProfile1.Email.Should().Be(profile1.Email);
        returnedProfile1.FullName.Should().Be(profile1.FullName);
        returnedProfile1.Age.Should().Be(profile1.Age);
        returnedProfile1.Gender.Should().Be(profile1.Gender);
        returnedProfile1.University.Should().Be(profile1.University);
        returnedProfile1.Bio.Should().Be(profile1.Bio);
        returnedProfile1.Lifestyle.Should().Be(profile1.Lifestyle);
        returnedProfile1.Interests.Should().Be(profile1.Interests);
        returnedProfile1.CreatedAt.Should().Be(profile1.CreatedAt);

        var returnedProfile2 = result.First(p => p.Id == profile2.Id);
        returnedProfile2.FullName.Should().Be(profile2.FullName);
        returnedProfile2.Age.Should().Be(profile2.Age);
        
        var returnedProfile3 = result.First(p => p.Id == profile3.Id);
        returnedProfile3.FullName.Should().Be(profile3.FullName);
        returnedProfile3.Age.Should().Be(profile3.Age);
    }

    [Fact]
    public async Task Given_SingleProfile_When_HandleIsCalled_Then_SingleProfileIsReturned()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "single@example.com",
            PasswordHash = "hashedpass",
            FullName = "Single User",
            Age = 28,
            Gender = "F",
            University = "Single University",
            Bio = "Single bio",
            Lifestyle = "Moderate",
            Interests = "Photography",
            CreatedAt = DateTime.UtcNow
        };

        context.Profiles.Add(profile);
        await context.SaveChangesAsync();

        var handler = new GetProfilesHandler(context);
        var request = new GetProfilesRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        
        var returnedProfile = result.First();
        returnedProfile.Id.Should().Be(profile.Id);
        returnedProfile.Email.Should().Be(profile.Email);
        returnedProfile.FullName.Should().Be(profile.FullName);
        returnedProfile.Age.Should().Be(profile.Age);
    }

}
