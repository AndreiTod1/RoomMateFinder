using MediatR;

namespace RoomMate_Finder.Features.Matching.GetUserMatches;

public record GetUserMatchesRequest(
    Guid UserId
) : IRequest<List<GetUserMatchesResponse>>;
