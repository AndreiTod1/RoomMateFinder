using MediatR;

namespace RoomMate_Finder.Features.RoomListings.ApproveRejectListing;

public record ApproveListingCommand(Guid ListingId, Guid AdminId) : IRequest<ApproveListingResponse>;

public record ApproveListingResponse(bool Success, string Message);


