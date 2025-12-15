using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.RoomListings.SearchListings;

public class SearchListingsHandler : IRequestHandler<SearchListingsRequest, SearchListingsResponse>
{
    private readonly AppDbContext _dbContext;

    public SearchListingsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SearchListingsResponse> Handle(SearchListingsRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.RoomListings
            .Include(l => l.Owner)
            .AsQueryable();

        // Filter by IsActive unless IncludeInactive is true
        if (!request.IncludeInactive)
        {
            query = query.Where(l => l.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            var city = request.City.Trim().ToLower();
            query = query.Where(l => l.City.ToLower() == city);
        }

        if (!string.IsNullOrWhiteSpace(request.Area))
        {
            var area = request.Area.Trim().ToLower();
            query = query.Where(l => l.Area.ToLower() == area);
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(l => l.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(l => l.Price <= request.MaxPrice.Value);
        }

        if (request.AvailableFrom.HasValue)
        {
            var from = request.AvailableFrom.Value.Date;
            query = query.Where(l => l.AvailableFrom.Date >= from);
        }

        if (request.Amenities is { Count: > 0 })
        {
            var requested = request.Amenities
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Select(a => a.Trim().ToLower())
                .ToList();

            foreach (var amenity in requested)
            {
                query = query.Where(l => l.Amenities.ToLower().Contains(amenity));
            }
        }

        if (request.OwnerId.HasValue)
        {
            query = query.Where(l => l.OwnerId == request.OwnerId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var listings = await query
            .OrderBy(l => l.AvailableFrom)
            .ThenByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new RoomListingSummaryDto
            {
                Id = l.Id,
                OwnerId = l.OwnerId,
                OwnerFullName = l.Owner.FullName,
                Title = l.Title,
                City = l.City,
                Area = l.Area,
                Price = l.Price,
                AvailableFrom = l.AvailableFrom,
                Amenities = l.Amenities
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList(),
                IsActive = l.IsActive,
                ThumbnailPath = l.ImagePaths.Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return new SearchListingsResponse
        {
            Listings = listings,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
