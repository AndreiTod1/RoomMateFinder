using MediatR;

namespace RoomMate_Finder.Features.Profiles.UpdateProfile;

public record UpdateProfileRequest(
    string? FullName,
    int? Age,
    string? Gender,
    string? University,
    string? Bio,
    string? Lifestyle,
    string? Interests
) : IRequest<UpdateProfileResponse>
{
    public Guid UserId { get; set; }
}
