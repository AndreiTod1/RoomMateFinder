using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Roommates.GetPendingRequests;

public class GetPendingRequestsHandler : IRequestHandler<GetPendingRequestsRequest, List<PendingRequestDto>>
{
    private readonly AppDbContext _context;

    public GetPendingRequestsHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PendingRequestDto>> Handle(GetPendingRequestsRequest request, CancellationToken cancellationToken)
    {
        // Only show requests where both users have confirmed (MutuallyConfirmed status)
        // Group by user pair to avoid showing duplicate requests
        var pendingRequests = await _context.RoommateRequests
            .Include(r => r.Requester)
            .Include(r => r.TargetUser)
            .Where(r => r.Status == RoommateRequestStatus.MutuallyConfirmed)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new PendingRequestDto(
                r.Id,
                r.RequesterId,
                r.Requester.FullName,
                r.Requester.Email,
                r.TargetUserId,
                r.TargetUser.FullName,
                r.TargetUser.Email,
                r.Message,
                r.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        // Group by user pair and return only one request per pair
        var distinctRequests = pendingRequests
            .GroupBy(r => r.RequesterId < r.TargetUserId 
                ? (r.RequesterId, r.TargetUserId) 
                : (r.TargetUserId, r.RequesterId))
            .Select(g => g.First())
            .ToList();

        return distinctRequests;
    }
}

