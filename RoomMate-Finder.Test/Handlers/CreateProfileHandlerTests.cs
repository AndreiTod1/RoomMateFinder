using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Common;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Profiles;
using RoomMate_Finder.Test.Helpers;
using RoomMate_Finder.Validators;
using Xunit;

namespace RoomMate_Finder.Test.Handlers;

public class CreateProfileHandlerTests : IDisposable
{
    private static JwtService CreateTestJwtService()
    {
        // Cheie suficient de lungă pentru HS256 (>= 256 bits / 32 bytes)
        const string key = "test_secret_key_for_jwt_signing_123456";
        const string issuer = "test-issuer";
        const string audience = "test-audience";
        return new JwtService(key, issuer, audience);
    }

    private static IValidator<CreateProfileRequest> CreateValidator()
    {
        return new CreateProfileValidator();
    }

    private static CreateProfileRequest CreateValidRequest(string email = "test@example.com")
    {
        return new CreateProfileRequest(
            Email: email,
            Password: "Str0ng!Pass1!",
            FullName: "Test User",
            Bio: "Test bio",
            Age: 25,
            Gender: "M",
            University: "Test University",
            Lifestyle: "Calm",
            Interests: "Coding, Music");
    }

    [Fact]
    public async Task Given_InvalidRequest_When_HandleIsCalled_Then_InvalidOperationExceptionWithValidationErrorsIsThrown()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var jwtService = CreateTestJwtService();
        var validator = CreateValidator();
        var handler = new CreateProfileHandler(context, jwtService, validator);

        var invalidRequest = new CreateProfileRequest(
            Email: "",            // invalid email
            Password: "weak",      // invalid password
            FullName: "",          // invalid name
            Bio: new string('a', 600),
            Age: 10,
            Gender: "",
            University: "",
            Lifestyle: new string('b', 200),
            Interests: new string('c', 300));

        // Act
        Func<Task> act = () => handler.Handle(invalidRequest, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.Which.Message.Should().Contain("'Full Name' must not be empty.");
        ex.Which.Message.Should().Contain("'Age' must be between 16 and 100.");
        ex.Which.Message.Should().Contain("A valid email address is required.");
    }

    [Fact]
    public async Task Given_EmailAlreadyExists_When_HandleIsCalled_Then_InvalidOperationExceptionIsThrown()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();

        var existingProfile = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "duplicate@example.com",
            PasswordHash = "hashed",
            FullName = "Existing User",
            Age = 30,
            Gender = "F",
            University = "Existing Uni",
            Bio = "Existing bio",
            Lifestyle = "Active",
            Interests = "Reading",
            CreatedAt = DateTime.UtcNow
        };
        context.Profiles.Add(existingProfile);
        await context.SaveChangesAsync();

        var jwtService = CreateTestJwtService();
        var validator = CreateValidator();
        var handler = new CreateProfileHandler(context, jwtService, validator);
        var request = CreateValidRequest(email: existingProfile.Email);

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        var exceptionAssertions = await act.Should().ThrowAsync<InvalidOperationException>();
        exceptionAssertions.Which.Message.Should().Contain("Email already registered");
    }

    [Fact]
    public async Task Given_ValidRequest_When_HandleIsCalled_Then_ProfileIsCreatedAndAuthResponseReturned()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var jwtService = CreateTestJwtService();
        var validator = CreateValidator();
        var handler = new CreateProfileHandler(context, jwtService, validator);
        var request = CreateValidRequest(email: "test1@gmail.com");

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().NotBe(Guid.Empty);
        result.Email.Should().Be(request.Email);
        result.FullName.Should().Be(request.FullName);
        result.Token.Should().NotBeNullOrWhiteSpace();

        var createdProfile = await context.Profiles.FirstOrDefaultAsync(p => p.Email == request.Email);
        createdProfile.Should().NotBeNull();
        createdProfile!.FullName.Should().Be(request.FullName);
        createdProfile.Email.Should().Be(request.Email);
    }

    public void Dispose()
    {
        // Clean up resources if needed
    }
}
