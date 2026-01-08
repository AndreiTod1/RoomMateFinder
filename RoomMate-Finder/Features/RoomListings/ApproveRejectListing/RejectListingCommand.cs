using MediatR;

namespace RoomMate_Finder.Features.RoomListings.ApproveRejectListing;

public record RejectListingCommand(Guid ListingId, Guid AdminId, string Reason) : IRequest<RejectListingResponse>;

public record RejectListingResponse(bool Success, string Message);

