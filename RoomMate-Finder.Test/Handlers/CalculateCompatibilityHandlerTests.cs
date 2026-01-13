using FluentAssertions;
using Moq;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Matching.CalculateCompatibility;
using RoomMate_Finder.Features.Matching.CalculateCompatibility.Services;
using RoomMate_Finder.Test.Helpers;

namespace RoomMate_Finder.Test.Handlers;

public class CalculateCompatibilityHandlerTests
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
            Bio = "Bio",
            Lifestyle = "quiet",
            Interests = "music, sports",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Given_NonExistentUser1_When_HandleIsCalled_Then_ThrowsArgumentException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user2 = CreateTestProfile(name: "User 2");
        context.Profiles.Add(user2);
        await context.SaveChangesAsync();

        var mockCalculator = new Mock<ICompatibilityCalculatorService>();
        var mockDescription = new Mock<ICompatibilityDescriptionService>();

        var handler = new CalculateCompatibilityHandler(context, mockCalculator.Object, mockDescription.Object);
        var request = new CalculateCompatibilityRequest(Guid.NewGuid(), user2.Id);

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Given_NonExistentUser2_When_HandleIsCalled_Then_ThrowsArgumentException()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user1 = CreateTestProfile(name: "User 1");
        context.Profiles.Add(user1);
        await context.SaveChangesAsync();

        var mockCalculator = new Mock<ICompatibilityCalculatorService>();
        var mockDescription = new Mock<ICompatibilityDescriptionService>();

        var handler = new CalculateCompatibilityHandler(context, mockCalculator.Object, mockDescription.Object);
        var request = new CalculateCompatibilityRequest(user1.Id, Guid.NewGuid());

        // Act
        Func<Task> act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Given_BothUsersExist_When_HandleIsCalled_Then_ReturnsCompatibilityResponse()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user1 = CreateTestProfile(name: "User 1");
        var user2 = CreateTestProfile(name: "User 2");
        context.Profiles.AddRange(user1, user2);
        await context.SaveChangesAsync();

        var compatibilityResult = new CompatibilityResult(
            AgeScore: 90,
            GenderScore: 80,
            UniversityScore: 100,
            LifestyleScore: 75,
            InterestsScore: 85,
            OverallScore: 86.0,
            CompatibilityLevel: "High"
        );

        var mockCalculator = new Mock<ICompatibilityCalculatorService>();
        mockCalculator.Setup(c => c.CalculateCompatibility(It.IsAny<Profile>(), It.IsAny<Profile>()))
            .Returns(compatibilityResult);

        var details = new CompatibilityDetails(
            AgeScore: 90,
            GenderScore: 80,
            UniversityScore: 100,
            LifestyleScore: 75,
            InterestsScore: 85,
            AgeDescription: "Same age group",
            GenderDescription: "Same gender",
            UniversityDescription: "Same university",
            LifestyleDescription: "Compatible lifestyles",
            InterestsDescription: "Many common interests"
        );

        var mockDescription = new Mock<ICompatibilityDescriptionService>();
        mockDescription.Setup(d => d.CreateDetails(It.IsAny<Profile>(), It.IsAny<Profile>(), compatibilityResult))
            .Returns(details);

        var handler = new CalculateCompatibilityHandler(context, mockCalculator.Object, mockDescription.Object);
        var request = new CalculateCompatibilityRequest(user1.Id, user2.Id);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId1.Should().Be(user1.Id);
        result.UserId2.Should().Be(user2.Id);
        result.CompatibilityScore.Should().Be(86.0);
        result.CompatibilityLevel.Should().Be("High");
        result.Details.Should().NotBeNull();
        result.Details.AgeScore.Should().Be(90);
    }

    [Fact]
    public async Task Given_ValidUsers_When_HandleIsCalled_Then_CalculatorServiceIsCalled()
    {
        // Arrange
        using var context = DbContextHelper.CreateInMemoryDbContext();
        var user1 = CreateTestProfile(name: "User 1");
        var user2 = CreateTestProfile(name: "User 2");
        context.Profiles.AddRange(user1, user2);
        await context.SaveChangesAsync();

        var compatibilityResult = new CompatibilityResult(80, 70, 90, 60, 50, 70.0, "Good");

        var mockCalculator = new Mock<ICompatibilityCalculatorService>();
        mockCalculator.Setup(c => c.CalculateCompatibility(It.IsAny<Profile>(), It.IsAny<Profile>()))
            .Returns(compatibilityResult);

        var mockDetails = new CompatibilityDetails(80, 70, 90, 60, 50, "", "", "", "", "");
        var mockDescription = new Mock<ICompatibilityDescriptionService>();
        mockDescription.Setup(d => d.CreateDetails(It.IsAny<Profile>(), It.IsAny<Profile>(), compatibilityResult))
            .Returns(mockDetails);

        var handler = new CalculateCompatibilityHandler(context, mockCalculator.Object, mockDescription.Object);
        var request = new CalculateCompatibilityRequest(user1.Id, user2.Id);

        // Act
        await handler.Handle(request, CancellationToken.None);

        // Assert
        mockCalculator.Verify(c => c.CalculateCompatibility(
            It.Is<Profile>(p => p.Id == user1.Id),
            It.Is<Profile>(p => p.Id == user2.Id)
        ), Times.Once);
    }
}

