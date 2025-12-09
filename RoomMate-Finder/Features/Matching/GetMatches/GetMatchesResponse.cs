namespace RoomMate_Finder.Features.Matching.GetMatches;

public record GetMatchesResponse(
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
    string? ProfilePicturePath // added field for profile pictures
);
