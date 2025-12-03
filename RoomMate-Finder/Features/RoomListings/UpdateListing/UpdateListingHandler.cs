namespace RoomMate_Finder.Features.RoomListings.UpdateListing;

using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Infrastructure.Persistence;

public class UpdateListingHandler : IRequestHandler<UpdateListingRequest, UpdateListingResponse?>
{
    private readonly AppDbContext _dbContext;

    public UpdateListingHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UpdateListingResponse?> Handle(UpdateListingRequest request, CancellationToken cancellationToken)
    {
        var listing = await _dbContext.RoomListings
            .FirstOrDefaultAsync(l => l.Id == request.Id && l.OwnerId == request.OwnerId, cancellationToken);

        if (listing == null)
        {
            return null;
        }

        listing.Title = request.Title;
        listing.Description = request.Description;
        listing.City = request.City;
        listing.Area = request.Area;
        listing.Price = request.Price;
        listing.AvailableFrom = request.AvailableFrom;
        listing.Amenities = string.Join(",", request.Amenities
            .Select(a => a.Trim())
            .Where(a => !string.IsNullOrWhiteSpace(a)));
        listing.IsActive = request.IsActive;
        listing.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateListingResponse
        {
            Id = listing.Id,
            OwnerId = listing.OwnerId,
            Title = listing.Title,
            Description = listing.Description,
            City = listing.City,
            Area = listing.Area,
            Price = listing.Price,
            AvailableFrom = listing.AvailableFrom,
            Amenities = listing.Amenities
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList(),
            CreatedAt = listing.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            IsActive = listing.IsActive
        };
    }
}
