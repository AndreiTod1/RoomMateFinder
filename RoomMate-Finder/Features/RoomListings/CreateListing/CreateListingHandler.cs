using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.RoomListings.CreateListing;

public class CreateListingHandler : IRequestHandler<CreateListingRequest, CreateListingResponse>
{
    private readonly AppDbContext _dbContext;

    public CreateListingHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateListingResponse> Handle(CreateListingRequest request, CancellationToken cancellationToken)
    {
        var owner = await _dbContext.Profiles
            .FirstOrDefaultAsync(p => p.Id == request.OwnerId, cancellationToken);

        if (owner == null)
        {
            throw new InvalidOperationException("Owner profile not found.");
        }

        var listing = new RoomListing
        {
            Id = Guid.NewGuid(),
            OwnerId = request.OwnerId,
            Title = request.Title,
            Description = request.Description,
            City = request.City,
            Area = request.Area,
            Price = request.Price,
            AvailableFrom = request.AvailableFrom,
            Amenities = string.Join(",", request.Amenities
                .Select(a => a.Trim())
                .Where(a => !string.IsNullOrWhiteSpace(a))),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _dbContext.RoomListings.Add(listing);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateListingResponse
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
            IsActive = listing.IsActive
        };
    }
}
