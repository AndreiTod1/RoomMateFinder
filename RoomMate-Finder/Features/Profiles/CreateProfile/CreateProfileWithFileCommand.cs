using MediatR;
using Microsoft.AspNetCore.Http;
using RoomMate_Finder.Features.Profiles.Login;

namespace RoomMate_Finder.Features.Profiles.CreateProfile;

public record CreateProfileWithFileCommand(
    CreateProfileRequest Profile,
    IFormFile? ProfilePicture
) : IRequest<AuthResponse>;
