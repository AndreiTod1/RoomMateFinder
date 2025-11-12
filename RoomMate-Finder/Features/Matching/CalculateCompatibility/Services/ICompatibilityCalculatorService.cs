using RoomMate_Finder.Entities;

namespace RoomMate_Finder.Features.Matching.CalculateCompatibility.Services;

public interface ICompatibilityCalculatorService
{
    CompatibilityResult CalculateCompatibility(Profile user1, Profile user2);
}

public record CompatibilityResult(
    double AgeScore,
    double GenderScore,
    double UniversityScore,
    double LifestyleScore,
    double InterestsScore,
    double OverallScore,
    string CompatibilityLevel
);
