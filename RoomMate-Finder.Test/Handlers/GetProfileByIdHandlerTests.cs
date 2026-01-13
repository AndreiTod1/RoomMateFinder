using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Profiles.GetProfileById;
using RoomMate_Finder.Test.Helpers;
using Xunit;

namespace RoomMate_Finder.Test.Handlers;

public class GetProfileByIdHandlerTests : IDisposable
{
    private bool _disposed;

    [Fact]
    public async Task Given_NonexistentId_When_HandleIsCalled_Then_InvalidOperationExceptionIsThrown()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var handler = new GetProfileByIdHandler(context);
        var request = new GetProfileByIdRequest(Guid.NewGuid());

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Be("Profile not found");
    }

    [Fact]
    public async Task Given_ExistingId_When_HandleIsCalled_Then_ProfileIsReturned()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashedpass",
            FullName = "Test User",
            Age = 25,
            Gender = "M",
            University = "Test University",
            Bio = "Test bio",
            Lifestyle = "Active",
            Interests = "Sports, Music",
            CreatedAt = DateTime.UtcNow
        };

        context.Profiles.Add(profile);
        await context.SaveChangesAsync();

        var handler = new GetProfileByIdHandler(context);
        var request = new GetProfileByIdRequest(profile.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(profile.Id);
        result.Email.Should().Be(profile.Email);
        result.FullName.Should().Be(profile.FullName);
        result.Age.Should().Be(profile.Age);
        result.Gender.Should().Be(profile.Gender);
        result.University.Should().Be(profile.University);
        result.Bio.Should().Be(profile.Bio);
        result.Lifestyle.Should().Be(profile.Lifestyle);
        result.Interests.Should().Be(profile.Interests);
        result.CreatedAt.Should().Be(profile.CreatedAt);
    }

    [Fact]
    public async Task Given_MultipleProfiles_When_HandleIsCalledWithSpecificId_Then_CorrectProfileIsReturned()
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
            Interests = "Sports",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
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
            Interests = "Reading",
            CreatedAt = DateTime.UtcNow
        };

        context.Profiles.AddRange(profile1, profile2);
        await context.SaveChangesAsync();

        var handler = new GetProfileByIdHandler(context);
        var request = new GetProfileByIdRequest(profile2.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(profile2.Id);
        result.Email.Should().Be(profile2.Email);
        result.FullName.Should().Be("User Two");
        result.Age.Should().Be(30);
        result.Gender.Should().Be("F");
        result.University.Should().Be("University Two");
    }

    [Fact]
    public async Task Given_EmptyDatabase_When_HandleIsCalled_Then_InvalidOperationExceptionIsThrown()
    {
        // This test is functionally identical to testing with a non-existent ID
        // since an empty database will not contain any profiles
        await Given_NonexistentId_When_HandleIsCalled_Then_InvalidOperationExceptionIsThrown();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources if any
                // Currently no managed resources to dispose in this test class
            }

            // Dispose unmanaged resources (if any)
            
            _disposed = true;
        }
    }
}
