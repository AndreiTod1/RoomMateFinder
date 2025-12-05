namespace RoomMate_Finder.Features.Matching.LikeProfile;

public record LikeProfileResponse(
    bool Success,
    string Message,
    bool IsMatch = false,
    Guid? MatchId = null
);
