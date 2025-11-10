namespace RoomMate_Finder.Features.Profiles.GetProfileById;

public record GetProfileByIdResponse(
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
       