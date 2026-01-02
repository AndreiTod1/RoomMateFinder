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
        var pendingRequests = await _context.RoommateRequests
            .Include(r => r.Requester)
            .Include(r => r.TargetUser)
            .Where(r => r.Status == RoommateRequestStatus.Pending)
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

        return pendingRequests;
    }
}

