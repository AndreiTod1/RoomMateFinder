using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Profiles.UpdateProfile;
using RoomMate_Finder.Test.Helpers;
using Xunit;

namespace RoomMate_Finder.Test.Handlers;

public class UpdateProfileHandlerTests : IDisposable
{
    private static UpdateProfileRequest CreateValidRequest(Guid userId)
    {
        return new UpdateProfileRequest(
            FullName: "Updated Name",
            Age: 26,
            Gender: "F",
            University: "Updated University",
            Bio: "Updated bio",
            Lifestyle: "Updated lifestyle",
            Interests: "Updated interests")
        {
            UserId = userId
        };
    }

    [Fact]
    public async Task Given_NonexistentProfile_When_HandleIsCalled_Then_KeyNotFoundExceptionIsThrown()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var handler = new UpdateProfileHandler(context);

        var request = CreateValidRequest(Guid.NewGuid()); // Non-existent ID

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<KeyNotFoundException>();
        ex.Which.Message.Should().Be("Profile not found");
    }

    [Fact]
    public async Task Given_ValidRequest_When_HandleIsCalled_Then_ProfileIsUpdatedAndResponseReturned()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var originalProfile = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashedpass",
            FullName = "Original Name",
            Age = 25,
            Gender = "M",
            University = "Original University",
            Bio = "Original bio",
            Lifestyle = "Original lifestyle",
            Interests = "Original interests",
            CreatedAt = DateTime.UtcNow
        };
        context.Profiles.Add(originalProfile);
        await context.SaveChangesAsync();

        var handler = new UpdateProfileHandler(context);
        var request = CreateValidRequest(originalProfile.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(originalProfile.Id);
        result.Email.Should().Be(originalProfile.Email); // Unchanged
        result.FullName.Should().Be("Updated Name");
        result.Age.Should().Be(26);
        result.Gender.Should().Be("F");
        result.University.Should().Be("Updated University");
        result.Bio.Should().Be("Updated bio");
        result.Lifestyle.Should().Be("Updated lifestyle");
        result.Interests.Should().Be("Updated interests");
        result.CreatedAt.Should().Be(originalProfile.CreatedAt); // Unchanged

        // Verify changes were persisted
        var updatedProfile = await context.Profiles.FindAsync(originalProfile.Id);
        updatedProfile!.FullName.Should().Be("Updated Name");
        updatedProfile.Age.Should().Be(26);
        updatedProfile.Gender.Should().Be("F");
    }

    [Fact]
    public async Task Given_PartialUpdateRequest_When_HandleIsCalled_Then_OnlySpecifiedFieldsAreUpdated()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var originalProfile = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashedpass",
            FullName = "Original Name",
            Age = 25,
            Gender = "M",
            University = "Original University",
            Bio = "Original bio",
            Lifestyle = "Original lifestyle",
            Interests = "Original interests",
            CreatedAt = DateTime.UtcNow
        };
        context.Profiles.Add(originalProfile);
        await context.SaveChangesAsync();

        var handler = new UpdateProfileHandler(context);
        
        // Only update FullName and Age
        var request = new UpdateProfileRequest(
            FullName: "Only Name Updated",
            Age: 30,
            Gender: null,
            University: null,
            Bio: null,
            Lifestyle: null,
            Interests: null)
        {
            UserId = originalProfile.Id
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FullName.Should().Be("Only Name Updated");
        result.Age.Should().Be(30);
        // These should remain unchanged
        result.Gender.Should().Be("M");
        result.University.Should().Be("Original University");
        result.Bio.Should().Be("Original bio");
        result.Lifestyle.Should().Be("Original lifestyle");
        result.Interests.Should().Be("Original interests");
    }

    [Fact]
    public async Task Given_NullValuesInRequest_When_HandleIsCalled_Then_FieldsRemainUnchanged()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        
        var originalProfile = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hashedpass",
            FullName = "Original Name",
            Age = 25,
            Gender = "M",
            University = "Original University",
            Bio = "Original bio",
            Lifestyle = "Original lifestyle",
            Interests = "Original interests",
            CreatedAt = DateTime.UtcNow
        };
        context.Profiles.Add(originalProfile);
        await context.SaveChangesAsync();

        var handler = new UpdateProfileHandler(context);
        
        // All null values - nothing should change
        var request = new UpdateProfileRequest(
            FullName: null,
            Age: null,
            Gender: null,
            University: null,
            Bio: null,
            Lifestyle: null,
            Interests: null)
        {
            UserId = originalProfile.Id
        };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FullName.Should().Be("Original Name");
        result.Age.Should().Be(25);
        result.Gender.Should().Be("M");
        result.University.Should().Be("Original University");
        result.Bio.Should().Be("Original bio");
        result.Lifestyle.Should().Be("Original lifestyle");
        result.Interests.Should().Be("Original interests");
    }

    public void Dispose()
    {
    }
}
