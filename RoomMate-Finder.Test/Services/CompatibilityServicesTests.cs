using FluentAssertions;
using Moq;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Matching.CalculateCompatibility.Services;

namespace RoomMate_Finder.Test.Services;

#region Age Compatibility Service Tests

public class AgeCompatibilityServiceTests
{
    private readonly AgeCompatibilityService _service = new();

    [Theory]
    [InlineData(25, 25, 100.0)]  // Same age
    [InlineData(25, 26, 95.0)]   // 1 year difference
    [InlineData(25, 27, 85.0)]   // 2 years difference
    [InlineData(25, 28, 75.0)]   // 3 years difference
    [InlineData(25, 29, 65.0)]   // 4 years difference
    [InlineData(25, 30, 50.0)]   // 5 years difference
    public void Given_AgeValues_When_CalculateScoreIsCalled_Then_ReturnsExpectedScore(int age1, int age2, double expectedScore)
    {
        // Act
        var result = _service.CalculateScore(age1, age2);

        // Assert
        result.Should().Be(expectedScore);
    }

    [Theory]
    [InlineData(20, 26, 45.0)]  // 6 years = 50 - (6-5)*5 = 45
    [InlineData(20, 30, 25.0)]  // 10 years = 50 - (10-5)*5 = 25
    [InlineData(20, 35, 0.0)]   // 15 years = 50 - (15-5)*5 = 0
    public void Given_LargeAgeDifference_When_CalculateScoreIsCalled_Then_ReturnsDecreasedScore(int age1, int age2, double expectedScore)
    {
        // Act
        var result = _service.CalculateScore(age1, age2);

        // Assert
        result.Should().Be(expectedScore);
    }

    [Fact]
    public void Given_SameAge_When_GetDescriptionIsCalled_Then_ReturnsPerfectMatchMessage()
    {
        // Act
        var result = _service.GetDescription(25, 25);

        // Assert
        result.Should().Be("Same age - perfect match!");
    }

    [Theory]
    [InlineData(25, 26, "1 year(s) difference - very compatible")]
    [InlineData(25, 27, "2 year(s) difference - very compatible")]
    public void Given_SmallAgeDifference_When_GetDescriptionIsCalled_Then_ReturnsVeryCompatibleMessage(int age1, int age2, string expected)
    {
        // Act
        var result = _service.GetDescription(age1, age2);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Given_LargeAgeDifference_When_GetDescriptionIsCalled_Then_ReturnsSomeAgeGapMessage()
    {
        // Act
        var result = _service.GetDescription(20, 30);

        // Assert
        result.Should().Be("10 year(s) difference - some age gap");
    }
}

#endregion

#region Gender Compatibility Service Tests

public class GenderCompatibilityServiceTests
{
    private readonly GenderCompatibilityService _service = new();

    [Theory]
    [InlineData("Male", "Male", 80.0)]
    [InlineData("Female", "Female", 80.0)]
    [InlineData("male", "MALE", 80.0)]  // Case insensitive
    public void Given_SameGender_When_CalculateScoreIsCalled_Then_Returns80(string gender1, string gender2, double expectedScore)
    {
        // Act
        var result = _service.CalculateScore(gender1, gender2);

        // Assert
        result.Should().Be(expectedScore);
    }

    [Theory]
    [InlineData("Male", "Female", 60.0)]
    [InlineData("Female", "Male", 60.0)]
    [InlineData("Other", "Male", 60.0)]
    public void Given_DifferentGender_When_CalculateScoreIsCalled_Then_Returns60(string gender1, string gender2, double expectedScore)
    {
        // Act
        var result = _service.CalculateScore(gender1, gender2);

        // Assert
        result.Should().Be(expectedScore);
    }

    [Fact]
    public void Given_SameGender_When_GetDescriptionIsCalled_Then_ReturnsSameGenderMessage()
    {
        // Act
        var result = _service.GetDescription("Male", "Male");

        // Assert
        result.Should().Be("Same gender - often preferred for roommates");
    }

    [Fact]
    public void Given_DifferentGender_When_GetDescriptionIsCalled_Then_ReturnsDifferentGenderMessage()
    {
        // Act
        var result = _service.GetDescription("Male", "Female");

        // Assert
        result.Should().Be("Different genders - still compatible");
    }
}

#endregion

#region University Compatibility Service Tests

public class UniversityCompatibilityServiceTests
{
    private readonly UniversityCompatibilityService _service = new();

    [Theory]
    [InlineData("UBB", "UBB", 100.0)]
    [InlineData("ubb", "UBB", 100.0)]  // Case insensitive
    public void Given_SameUniversity_When_CalculateScoreIsCalled_Then_Returns100(string uni1, string uni2, double expectedScore)
    {
        // Act
        var result = _service.CalculateScore(uni1, uni2);

        // Assert
        result.Should().Be(expectedScore);
    }

    [Theory]
    [InlineData("UBB", "UTCN", 40.0)]
    [InlineData("Harvard", "MIT", 40.0)]
    public void Given_DifferentUniversity_When_CalculateScoreIsCalled_Then_Returns40(string uni1, string uni2, double expectedScore)
    {
        // Act
        var result = _service.CalculateScore(uni1, uni2);

        // Assert
        result.Should().Be(expectedScore);
    }

    [Fact]
    public void Given_SameUniversity_When_GetDescriptionIsCalled_Then_ReturnsSameUniversityMessage()
    {
        // Act
        var result = _service.GetDescription("UBB", "UBB");

        // Assert
        result.Should().Be("Same university - great for commuting together");
    }

    [Fact]
    public void Given_DifferentUniversity_When_GetDescriptionIsCalled_Then_ReturnsDifferentUniversityMessage()
    {
        // Act
        var result = _service.GetDescription("UBB", "UTCN");

        // Assert
        result.Should().Be("Different universities - manageable");
    }
}

#endregion

#region Lifestyle Compatibility Service Tests

public class LifestyleCompatibilityServiceTests
{
    private readonly LifestyleCompatibilityService _service = new();

    [Theory]
    [InlineData("quiet", "quiet", 100.0)]
    [InlineData("social", "social", 100.0)]
    [InlineData("QUIET", "quiet", 100.0)]  // Case insensitive
    public void Given_SameLifestyle_When_CalculateScoreIsCalled_Then_Returns100(string life1, string life2, double expectedScore)
    {
        // Act
        var result = _service.CalculateScore(life1, life2);

        // Assert
        result.Should().Be(expectedScore);
    }

    [Theory]
    [InlineData("quiet", "studious", 75.0)]  // Compatible pair
    [InlineData("social", "outgoing", 75.0)] // Compatible pair
    public void Given_CompatibleLifestyles_When_CalculateScoreIsCalled_Then_Returns75(string life1, string life2, double expectedScore)
    {
        // Act
        var result = _service.CalculateScore(life1, life2);

        // Assert
        result.Should().Be(expectedScore);
    }

    [Theory]
    [InlineData("quiet", "social", 30.0)]   // Not compatible
    [InlineData("random", "other", 30.0)]   // Unknown types
    public void Given_IncompatibleLifestyles_When_CalculateScoreIsCalled_Then_Returns30(string life1, string life2, double expectedScore)
    {
        // Act
        var result = _service.CalculateScore(life1, life2);

        // Assert
        result.Should().Be(expectedScore);
    }

    [Fact]
    public void Given_SameLifestyle_When_GetDescriptionIsCalled_Then_ReturnsExcellentMessage()
    {
        // Act
        var result = _service.GetDescription("quiet", "quiet", 100.0);

        // Assert
        result.Should().Be("Same lifestyle - excellent compatibility");
    }

    [Fact]
    public void Given_CompatibleLifestyle_When_GetDescriptionIsCalled_Then_ReturnsCompatibleMessage()
    {
        // Act
        var result = _service.GetDescription("quiet", "studious", 75.0);

        // Assert
        result.Should().Be("Compatible lifestyles");
    }

    [Fact]
    public void Given_IncompatibleLifestyle_When_GetDescriptionIsCalled_Then_ReturnsCompromiseMessage()
    {
        // Act
        var result = _service.GetDescription("quiet", "social", 30.0);

        // Assert
        result.Should().Be("Different lifestyles - may need compromise");
    }
}

#endregion

#region Interests Compatibility Service Tests

public class InterestsCompatibilityServiceTests
{
    private readonly InterestsCompatibilityService _service = new();

    [Fact]
    public void Given_EmptyInterests_When_CalculateScoreIsCalled_Then_Returns50()
    {
        // Act
        var result = _service.CalculateScore("", "music, sports");

        // Assert
        result.Should().Be(50.0);
    }

    [Fact]
    public void Given_NullInterests_When_CalculateScoreIsCalled_Then_Returns50()
    {
        // Act
        var result = _service.CalculateScore(null!, "music");

        // Assert
        result.Should().Be(50.0);
    }

    [Fact]
    public void Given_IdenticalInterests_When_CalculateScoreIsCalled_Then_Returns100()
    {
        // Act
        var result = _service.CalculateScore("music, sports, gaming", "music, sports, gaming");

        // Assert
        result.Should().Be(100.0);
    }

    [Fact]
    public void Given_NoCommonInterests_When_CalculateScoreIsCalled_Then_Returns20()
    {
        // Act
        var result = _service.CalculateScore("music, sports", "cooking, reading");

        // Assert
        result.Should().Be(20.0);
    }

    [Fact]
    public void Given_PartiallyMatchingInterests_When_CalculateScoreIsCalled_Then_ReturnsProportionalScore()
    {
        // Arrange: 2 common out of 4 total = 50%
        var result = _service.CalculateScore("music, sports, gaming, reading", "music, sports, cooking, travel");

        // Assert
        result.Should().Be(50.0);
    }

    [Theory]
    [InlineData(70.0, "Many shared interests - great for bonding")]
    [InlineData(80.0, "Many shared interests - great for bonding")]
    [InlineData(50.0, "Some common interests - good foundation")]
    [InlineData(40.0, "Some common interests - good foundation")]
    [InlineData(20.0, "Different interests - opportunity to learn from each other")]
    [InlineData(30.0, "Different interests - opportunity to learn from each other")]
    public void Given_Score_When_GetDescriptionIsCalled_Then_ReturnsAppropriateMessage(double score, string expected)
    {
        // Act
        var result = _service.GetDescription(score);

        // Assert
        result.Should().Be(expected);
    }
}

#endregion

#region Compatibility Calculator Service Tests

public class CompatibilityCalculatorServiceTests
{
    private readonly Mock<IAgeCompatibilityService> _mockAgeService = new();
    private readonly Mock<IGenderCompatibilityService> _mockGenderService = new();
    private readonly Mock<IUniversityCompatibilityService> _mockUniversityService = new();
    private readonly Mock<ILifestyleCompatibilityService> _mockLifestyleService = new();
    private readonly Mock<IInterestsCompatibilityService> _mockInterestsService = new();

    private CompatibilityCalculatorService CreateService()
    {
        return new CompatibilityCalculatorService(
            _mockAgeService.Object,
            _mockGenderService.Object,
            _mockUniversityService.Object,
            _mockLifestyleService.Object,
            _mockInterestsService.Object
        );
    }

    private static Profile CreateTestProfile(int age = 25, string gender = "Male", string university = "UBB", 
        string lifestyle = "quiet", string interests = "music")
    {
        return new Profile
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            PasswordHash = "hash",
            FullName = "Test User",
            Age = age,
            Gender = gender,
            University = university,
            Lifestyle = lifestyle,
            Interests = interests,
            Bio = "Test bio",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void Given_TwoProfiles_When_CalculateCompatibilityIsCalled_Then_ReturnsWeightedResult()
    {
        // Arrange
        var user1 = CreateTestProfile();
        var user2 = CreateTestProfile();

        _mockAgeService.Setup(s => s.CalculateScore(It.IsAny<int>(), It.IsAny<int>())).Returns(100.0);
        _mockGenderService.Setup(s => s.CalculateScore(It.IsAny<string>(), It.IsAny<string>())).Returns(80.0);
        _mockUniversityService.Setup(s => s.CalculateScore(It.IsAny<string>(), It.IsAny<string>())).Returns(100.0);
        _mockLifestyleService.Setup(s => s.CalculateScore(It.IsAny<string>(), It.IsAny<string>())).Returns(100.0);
        _mockInterestsService.Setup(s => s.CalculateScore(It.IsAny<string>(), It.IsAny<string>())).Returns(100.0);

        var service = CreateService();

        // Act
        var result = service.CalculateCompatibility(user1, user2);

        // Assert
        // Weighted: (100*0.2) + (80*0.15) + (100*0.25) + (100*0.25) + (100*0.15) = 20 + 12 + 25 + 25 + 15 = 97
        result.OverallScore.Should().Be(97.0);
        result.AgeScore.Should().Be(100.0);
        result.GenderScore.Should().Be(80.0);
        result.UniversityScore.Should().Be(100.0);
        result.LifestyleScore.Should().Be(100.0);
        result.InterestsScore.Should().Be(100.0);
    }

    [Theory]
    [InlineData(90.0, "Excellent Match")]
    [InlineData(85.0, "Excellent Match")]
    [InlineData(75.0, "Very Good Match")]
    [InlineData(70.0, "Very Good Match")]
    [InlineData(60.0, "Good Match")]
    [InlineData(55.0, "Good Match")]
    [InlineData(45.0, "Moderate Match")]
    [InlineData(40.0, "Moderate Match")]
    [InlineData(30.0, "Low Compatibility")]
    [InlineData(20.0, "Low Compatibility")]
    public void Given_OverallScore_When_CalculateCompatibilityIsCalled_Then_ReturnsCorrectLevel(double overallScore, string expectedLevel)
    {
        // Arrange
        var user1 = CreateTestProfile();
        var user2 = CreateTestProfile();

        // Set all scores to achieve the target overall score
        // Since weighted average uses: age*0.2 + gender*0.15 + uni*0.25 + life*0.25 + int*0.15
        // Setting all to same value gives that value as overall
        _mockAgeService.Setup(s => s.CalculateScore(It.IsAny<int>(), It.IsAny<int>())).Returns(overallScore);
        _mockGenderService.Setup(s => s.CalculateScore(It.IsAny<string>(), It.IsAny<string>())).Returns(overallScore);
        _mockUniversityService.Setup(s => s.CalculateScore(It.IsAny<string>(), It.IsAny<string>())).Returns(overallScore);
        _mockLifestyleService.Setup(s => s.CalculateScore(It.IsAny<string>(), It.IsAny<string>())).Returns(overallScore);
        _mockInterestsService.Setup(s => s.CalculateScore(It.IsAny<string>(), It.IsAny<string>())).Returns(overallScore);

        var service = CreateService();

        // Act
        var result = service.CalculateCompatibility(user1, user2);

        // Assert
        result.CompatibilityLevel.Should().Be(expectedLevel);
    }
}

#endregion

#region Compatibility Description Service Tests

public class CompatibilityDescriptionServiceTests
{
    private readonly Mock<IAgeCompatibilityService> _mockAgeService = new();
    private readonly Mock<IGenderCompatibilityService> _mockGenderService = new();
    private readonly Mock<IUniversityCompatibilityService> _mockUniversityService = new();
    private readonly Mock<ILifestyleCompatibilityService> _mockLifestyleService = new();
    private readonly Mock<IInterestsCompatibilityService> _mockInterestsService = new();

    private CompatibilityDescriptionService CreateService()
    {
        return new CompatibilityDescriptionService(
            _mockAgeService.Object,
            _mockGenderService.Object,
            _mockUniversityService.Object,
            _mockLifestyleService.Object,
            _mockInterestsService.Object
        );
    }

    private static Profile CreateTestProfile(int age = 25, string gender = "Male", string university = "UBB",
        string lifestyle = "quiet", string interests = "music")
    {
        return new Profile
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            PasswordHash = "hash",
            FullName = "Test User",
            Age = age,
            Gender = gender,
            University = university,
            Lifestyle = lifestyle,
            Interests = interests,
            Bio = "Test bio",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public void Given_TwoProfilesAndResult_When_CreateDetailsIsCalled_Then_ReturnsComprehensiveDetails()
    {
        // Arrange
        var user1 = CreateTestProfile();
        var user2 = CreateTestProfile(age: 26, gender: "Female");
        var result = new CompatibilityResult(95.0, 60.0, 100.0, 75.0, 80.0, 82.0, "Very Good Match");

        _mockAgeService.Setup(s => s.GetDescription(user1.Age, user2.Age)).Returns("1 year(s) difference - very compatible");
        _mockGenderService.Setup(s => s.GetDescription(user1.Gender, user2.Gender)).Returns("Different genders - still compatible");
        _mockUniversityService.Setup(s => s.GetDescription(user1.University, user2.University)).Returns("Same university - great for commuting together");
        _mockLifestyleService.Setup(s => s.GetDescription(user1.Lifestyle, user2.Lifestyle, result.LifestyleScore)).Returns("Compatible lifestyles");
        _mockInterestsService.Setup(s => s.GetDescription(result.InterestsScore)).Returns("Many shared interests - great for bonding");

        var service = CreateService();

        // Act
        var details = service.CreateDetails(user1, user2, result);

        // Assert
        details.AgeScore.Should().Be(95.0);
        details.GenderScore.Should().Be(60.0);
        details.UniversityScore.Should().Be(100.0);
        details.LifestyleScore.Should().Be(75.0);
        details.InterestsScore.Should().Be(80.0);
        details.AgeDescription.Should().Be("1 year(s) difference - very compatible");
        details.GenderDescription.Should().Be("Different genders - still compatible");
        details.UniversityDescription.Should().Be("Same university - great for commuting together");
        details.LifestyleDescription.Should().Be("Compatible lifestyles");
        details.InterestsDescription.Should().Be("Many shared interests - great for bonding");
    }
}

#endregion
