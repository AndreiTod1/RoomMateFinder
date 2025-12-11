namespace RoomMate_Finder.Features.Admins.GetAdmins;

public record ProfileResponse(
    Guid Id,
    string Email,
    string FullName,
    int Age,
    string Gender,
    string University,
    string Bio,
    string Lifestyle,
    string Interests,
    string? ProfilePicturePath,
    DateTime CreatedAt,
    string Role
);
