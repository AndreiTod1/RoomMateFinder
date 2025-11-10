using MediatR;

namespace RoomMate_Finder.Features.Profiles.GetProfiles;

public record GetProfilesRequest() : IRequest<List<GetProfilesResponse>>;