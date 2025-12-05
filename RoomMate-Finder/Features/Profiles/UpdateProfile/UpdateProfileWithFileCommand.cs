using MediatR;
using Microsoft.AspNetCore.Http;

namespace RoomMate_Finder.Features.Profiles.UpdateProfile;

public record UpdateProfileWithFileCommand(
    UpdateProfileRequest Profile,
    IFormFile? ProfilePicture
) : IRequest<UpdateProfileResponse>;

