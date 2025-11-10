namespace RoomMate_Finder.Features.Profiles.UpdateProfile;

public record UpdateProfileResponse(
    Guid Id,
    string Email,
    string FullName,
    int Age,
    string Gender,
    string University,
    string Bio,
    string Lifestyle,
    string Interests,
    DateTime CreatedAt
);