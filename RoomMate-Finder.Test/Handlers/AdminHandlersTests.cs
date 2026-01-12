using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Admins.DeleteProfile;
using RoomMate_Finder.Features.Admins.GetAdmins;
using RoomMate_Finder.Features.Admins.GetAllUsers;
using RoomMate_Finder.Features.Admins.UpdateUserRole;
using RoomMate_Finder.Test.Helpers;

namespace RoomMate_Finder.Test.Handlers;

#region Delete Profile Handler Tests

public class DeleteProfileHandlerTests
{
    private static Profile CreateTestProfile(string email = "test@test.com", string role = "User")
    {
        return new Profile
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hashed",
            FullName = "Test User",
            Age = 25,
            Gender = "Male",
            University = "Test University",
            Bio = "Test bio",
            Lifestyle = "quiet",
            Interests = "music",
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Given_ExistingProfile_When_HandleIsCalled_Then_ProfileIsDeleted()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var profile = CreateTestProfile();
        context.Profiles.Add(profile);
        await context.SaveChangesAsync();

        var handler = new DeleteProfileHandler(context);
        var request = new DeleteProfileRequest(profile.Id);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        var deletedProfile = await context.Profiles.FindAsync(profile.Id);
        deletedProfile.Should().BeNull();
    }

    [Fact]
    public async Task Given_NonExistentProfile_When_HandleIsCalled_Then_KeyNotFoundExceptionIsThrown()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var handler = new DeleteProfileHandler(context);
        var request = new DeleteProfileRequest(Guid.NewGuid());

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }
}

#endregion

#region Get Admins Handler Tests

public class GetAdminsHandlerTests
{
    private static Profile CreateTestProfile(string email, string role)
    {
        return new Profile
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hashed",
            FullName = $"User {email}",
            Age = 25,
            Gender = "Male",
            University = "Test University",
            Bio = "Test bio",
            Lifestyle = "quiet",
            Interests = "music",
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Given_AdminsExist_When_HandleIsCalled_Then_ReturnsOnlyAdmins()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        context.Profiles.AddRange(
            CreateTestProfile("admin1@test.com", "Admin"),
            CreateTestProfile("admin2@test.com", "Admin"),
            CreateTestProfile("user1@test.com", "User")
        );
        await context.SaveChangesAsync();

        var handler = new GetAdminsHandler(context);
        var request = new GetAdminsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.Role.Should().Be("Admin"));
    }

    [Fact]
    public async Task Given_NoAdmins_When_HandleIsCalled_Then_ReturnsEmptyList()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        context.Profiles.Add(CreateTestProfile("user@test.com", "User"));
        await context.SaveChangesAsync();

        var handler = new GetAdminsHandler(context);
        var request = new GetAdminsRequest();

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}

#endregion

#region Get All Users Handler Tests

public class GetAllUsersHandlerTests
{
    private static Profile CreateTestProfile(string email, string fullName, string role = "User")
    {
        return new Profile
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hashed",
            FullName = fullName,
            Age = 25,
            Gender = "Male",
            University = "Test University",
            Bio = "Test bio",
            Lifestyle = "quiet",
            Interests = "music",
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Given_UsersExist_When_HandleIsCalled_Then_ReturnsPaginatedResults()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        for (int i = 1; i <= 15; i++)
        {
            context.Profiles.Add(CreateTestProfile($"user{i}@test.com", $"User {i}"));
        }
        await context.SaveChangesAsync();

        var handler = new GetAllUsersHandler(context);
        var request = new GetAllUsersRequest(1, 10, null);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Users.Should().HaveCount(10);
        result.TotalCount.Should().Be(15);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Given_SearchQuery_When_HandleIsCalled_Then_ReturnsFilteredResults()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        context.Profiles.AddRange(
            CreateTestProfile("john@test.com", "John Doe"),
            CreateTestProfile("jane@test.com", "Jane Smith"),
            CreateTestProfile("bob@test.com", "Bob Johnson")
        );
        await context.SaveChangesAsync();

        var handler = new GetAllUsersHandler(context);
        var request = new GetAllUsersRequest(1, 10, "john");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Users.Should().HaveCount(2); // John Doe and Bob Johnson
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Given_AdminsAndUsers_When_HandleIsCalled_Then_AdminsAppearFirst()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        context.Profiles.AddRange(
            CreateTestProfile("user@test.com", "Alice User", "User"),
            CreateTestProfile("admin@test.com", "Zack Admin", "Admin")
        );
        await context.SaveChangesAsync();

        var handler = new GetAllUsersHandler(context);
        var request = new GetAllUsersRequest(1, 10, null);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Users.First().Role.Should().Be("Admin");
    }
}

#endregion

#region Update User Role Handler Tests

public class UpdateUserRoleHandlerTests
{
    private static Profile CreateTestProfile(string role = "User")
    {
        return new Profile
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            PasswordHash = "hashed",
            FullName = "Test User",
            Age = 25,
            Gender = "Male",
            University = "Test University",
            Bio = "Test bio",
            Lifestyle = "quiet",
            Interests = "music",
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Given_ValidRequest_When_HandleIsCalled_Then_RoleIsUpdated()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var profile = CreateTestProfile("User");
        context.Profiles.Add(profile);
        await context.SaveChangesAsync();

        var handler = new UpdateUserRoleHandler(context);
        var request = new UpdateUserRoleRequest(profile.Id, "Admin");

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        var updatedProfile = await context.Profiles.FindAsync(profile.Id);
        updatedProfile!.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Given_NonExistentProfile_When_HandleIsCalled_Then_KeyNotFoundExceptionIsThrown()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var handler = new UpdateUserRoleHandler(context);
        var request = new UpdateUserRoleRequest(Guid.NewGuid(), "Admin");

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    [Theory]
    [InlineData("InvalidRole")]
    [InlineData("SuperAdmin")]
    [InlineData("")]
    public async Task Given_InvalidRole_When_HandleIsCalled_Then_ArgumentExceptionIsThrown(string invalidRole)
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var profile = CreateTestProfile();
        context.Profiles.Add(profile);
        await context.SaveChangesAsync();

        var handler = new UpdateUserRoleHandler(context);
        var request = new UpdateUserRoleRequest(profile.Id, invalidRole);

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Invalid role*");
    }
}

#endregion
