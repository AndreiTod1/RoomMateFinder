using MediatR;

namespace RoomMate_Finder.Features.RoomListings.GetListingById;

public record GetListingByIdRequest(Guid Id) : IRequest<GetListingByIdResponse?>;

