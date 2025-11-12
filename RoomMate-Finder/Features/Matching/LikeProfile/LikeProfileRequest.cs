using MediatR;

namespace RoomMate_Finder.Features.Matching.LikeProfile;

public record LikeProfileRequest(
    Guid UserId,
    Guid TargetUserId
) : IRequest<LikeProfileResponse>;
