namespace RoomMate_Finder.Features.Profiles.Login;

public record AuthResponse(
    Guid UserId, 
    string Email, 
    string FullName, 
    string Token,
    string? ProfilePicturePath = null,
    string Message = "Success"
);
