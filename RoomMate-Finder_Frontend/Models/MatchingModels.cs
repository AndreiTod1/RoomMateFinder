using System;

namespace RoomMate_Finder_Frontend.Models;

public record MatchProfileDto(
    Guid UserId,
    string Email,
    string FullName,
    int Age,
    string Gender,
    string University,
    string Bio,
    string Lifestyle,
    string Interests,
    double CompatibilityScore,
    string CompatibilityLevel,
    DateTime CreatedAt,
    string? ProfilePicturePath = null
);

public record CompatibilityDetailsDto(
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

public record CompatibilityDto(
    Guid UserId1,
    Guid UserId2,
    double CompatibilityScore,
    string CompatibilityLevel,
    CompatibilityDetailsDto Details
);

public record UserMatchDto(
    Guid MatchId,
    Guid UserId,
    string Email,
    string FullName,
    int Age,
    string Gender,
    string University,
    string Bio,
    string Lifestyle,
    string Interests,
    DateTime MatchedAt,
    bool IsActive,
    string? ProfilePicturePath = null
);

public record LikeResponseDto(bool Success, string Message, bool IsMatch = false, Guid? MatchId = null);
public record PassResponseDto(bool Success, string Message);

