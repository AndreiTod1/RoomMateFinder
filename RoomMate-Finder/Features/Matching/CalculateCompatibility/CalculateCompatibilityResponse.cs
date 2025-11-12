namespace RoomMate_Finder.Features.Matching.CalculateCompatibility;

public record CalculateCompatibilityResponse(
    Guid UserId1,
    Guid UserId2,
    double CompatibilityScore,
    string CompatibilityLevel,
    CompatibilityDetails Details
);

public record CompatibilityDetails(
    double AgeScore,
    double GenderScore,
    double UniversityScore,
    double LifestyleScore,
    double InterestsScore,
    string AgeDescription,
    string GenderDescription,
    string UniversityDescription,
    string LifestyleDescription,
    string InterestsDescription
);
