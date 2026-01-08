using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.RoomListings.ApproveRejectListing;

public class RejectListingHandler : IRequestHandler<RejectListingCommand, RejectListingResponse>
{
    private readonly AppDbContext _dbContext;

    public RejectListingHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RejectListingResponse> Handle(RejectListingCommand command, CancellationToken cancellationToken)
    {
        var listing = await _dbContext.RoomListings
            .FirstOrDefaultAsync(l => l.Id == command.ListingId, cancellationToken);

        if (listing == null)
        {
            return new RejectListingResponse(false, "Listing not found.");
        }

        if (listing.ApprovalStatus == ListingApprovalStatus.Rejected)
        {
            return new RejectListingResponse(false, "Listing is already rejected.");
        }

        listing.ApprovalStatus = ListingApprovalStatus.Rejected;
        listing.ApprovedByAdminId = command.AdminId;
        listing.ApprovedAt = DateTime.UtcNow;
        listing.RejectionReason = command.Reason;
        listing.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RejectListingResponse(true, "Listing rejected.");
    }
}

