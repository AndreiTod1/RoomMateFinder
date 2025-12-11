using MediatR;

namespace RoomMate_Finder.Features.Profiles.GetProfileById;

public record GetProfileByIdRequest(Guid Id) : IRequest<GetProfileByIdResponse>;