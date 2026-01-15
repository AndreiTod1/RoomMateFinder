using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.RoomListings.DeleteListing;

public record DeleteListingCommand(Guid ListingId, Guid UserId) : IRequest<DeleteListingResult>;

public record DeleteListingResult(bool Success, string Message);

public class DeleteListingHandler : IRequestHandler<DeleteListingCommand, DeleteListingResult>
{
    private readonly AppDbContext _context;

    public DeleteListingHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteListingResult> Handle(DeleteListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await _context.RoomListings.FindAsync(new object[] { request.ListingId }, cancellationToken);

        if (listing == null)
        {
            return new DeleteListingResult(false, "Listing not found");
        }

        var userProfile = await _context.Profiles.FindAsync(new object[] { request.UserId }, cancellationToken);
        var isAdmin = userProfile?.Role == "Admin";

        if (listing.OwnerId != request.UserId && !isAdmin)
        {
            return new DeleteListingResult(false, "You are not authorized to delete this listing");
        }

        _context.RoomListings.Remove(listing);
        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteListingResult(true, "Listing deleted successfully");
    }
}
