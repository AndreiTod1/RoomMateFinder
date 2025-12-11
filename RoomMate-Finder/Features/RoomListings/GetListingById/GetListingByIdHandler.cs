namespace RoomMate_Finder.Features.RoomListings.GetListingById;

using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Infrastructure.Persistence;

public class GetListingByIdHandler : IRequestHandler<GetListingByIdRequest, GetListingByIdResponse?>
{
    private readonly AppDbContext _dbContext;

    public GetListingByIdHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetListingByIdResponse?> Handle(GetListingByIdRequest request, CancellationToken cancellationToken)
    {
        var listing = await _dbContext.RoomListings
            .Include(l => l.Owner)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (listing == null)
        {
            return null;
        }

        return new GetListingByIdResponse
        {
            Id = listing.Id,
            OwnerId = listing.OwnerId,
            OwnerFullName = listing.Owner.FullName,
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
            UpdatedAt = listing.UpdatedAt,
            IsActive = listing.IsActive
        };
    }
}
