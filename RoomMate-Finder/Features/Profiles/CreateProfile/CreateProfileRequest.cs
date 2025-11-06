using MediatR;

namespace RoomMate_Finder.Features.Profiles;

public record CreateProfileRequest (
    Guid UserId,
    string FullName,
    string Bio,
    int Age,
    string Gender,
    string University,
    string Lifestyle,
    string Interests
    ) : IRequest<Guid>;