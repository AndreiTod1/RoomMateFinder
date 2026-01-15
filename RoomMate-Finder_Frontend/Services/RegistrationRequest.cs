namespace RoomMate_Finder_Frontend.Services;

/// <summary>
/// Represents a user registration request with profile information.
/// </summary>
public record RegistrationRequest(
    string Email,
    string Password,
    string FullName,
    int Age,
    string Gender,
    string University,
    string Bio,
    string Lifestyle,
    string Interests,
    string? ProfilePictureUrl
);
