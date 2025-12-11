using MediatR;

namespace RoomMate_Finder.Features.Admins.DeleteProfile;

public record DeleteProfileRequest(Guid Id) : IRequest;
