using MediatR;
using RoomMate_Finder.Features.Profiles;

namespace RoomMate_Finder.Features.Admins.GetAdmins;

public record GetAdminsRequest : IRequest<List<ProfileResponse>>;
