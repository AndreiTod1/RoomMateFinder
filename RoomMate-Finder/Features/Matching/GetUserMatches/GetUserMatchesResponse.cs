namespace RoomMate_Finder.Features.Matching.GetUserMatches;

public record GetUserMatchesResponse(
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
    bool IsActive
);
