using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.RoomListings.ApproveRejectListing;

public class ApproveListingHandler : IRequestHandler<ApproveListingCommand, ApproveListingResponse>
{
    private readonly AppDbContext _dbContext;

    public ApproveListingHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApproveListingResponse> Handle(ApproveListingCommand command, CancellationToken cancellationToken)
    {
        var listing = await _dbContext.RoomListings
            .FirstOrDefaultAsync(l => l.Id == command.ListingId, cancellationToken);

        if (listing == null)
        {
            return new ApproveListingResponse(false, "Listing not found.");
        }

        if (listing.ApprovalStatus == ListingApprovalStatus.Approved)
        {
            return new ApproveListingResponse(false, "Listing is already approved.");
        }

        listing.ApprovalStatus = ListingApprovalStatus.Approved;
        listing.ApprovedByAdminId = command.AdminId;
        listing.ApprovedAt = DateTime.UtcNow;
        listing.RejectionReason = null;
        listing.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApproveListingResponse(true, "Listing approved successfully.");
    }
}

