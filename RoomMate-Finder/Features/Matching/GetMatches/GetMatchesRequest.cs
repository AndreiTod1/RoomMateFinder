using MediatR;

namespace RoomMate_Finder.Features.Matching.GetMatches;

public record GetMatchesRequest(
    Guid UserId
) : IRequest<List<GetMatchesResponse>>;
