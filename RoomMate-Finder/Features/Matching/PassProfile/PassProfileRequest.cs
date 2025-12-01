using MediatR;

namespace RoomMate_Finder.Features.Matching.PassProfile;

public record PassProfileRequest(
    Guid UserId,
    Guid TargetUserId
) : IRequest<PassProfileResponse>;
