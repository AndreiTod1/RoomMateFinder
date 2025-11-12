using MediatR;

namespace RoomMate_Finder.Features.Matching.CalculateCompatibility;

public record CalculateCompatibilityRequest(
    Guid UserId1,
    Guid UserId2
) : IRequest<CalculateCompatibilityResponse>;
