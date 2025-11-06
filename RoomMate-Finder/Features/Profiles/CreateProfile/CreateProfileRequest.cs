using MediatR;
using RoomMate_Finder.Features.Profiles.Login;

namespace RoomMate_Finder.Features.Profiles;

public record CreateProfileRequest (
    string Email,
    string Password,
    string FullName,
    string Bio,
    int Age,
    string Gender,
    string University,
    string Lifestyle,
    string Interests
    ) : IRequest<AuthResponse>;
