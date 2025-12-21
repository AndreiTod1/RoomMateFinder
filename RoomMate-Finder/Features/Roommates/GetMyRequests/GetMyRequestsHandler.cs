using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Infrastructure.Persistence;
using System.Security.Claims;

namespace RoomMate_Finder.Features.Roommates.GetMyRequests;

public class GetMyRequestsHandler : IRequestHandler<GetMyRequestsRequest, GetMyRequestsResponse>
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetMyRequestsHandler(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<GetMyRequestsResponse> Handle(GetMyRequestsRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        // Get sent requests
        var sentRequests = await _context.RoommateRequests
            .Include(r => r.TargetUser)
            .Where(r => r.RequesterId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new MyRequestDto(
                r.Id,
                r.TargetUserId,
                r.TargetUser.FullName,
                r.TargetUser.Email,
                r.Message,
                r.Status.ToString(),
                r.CreatedAt,
                r.ProcessedAt
            ))
            .ToListAsync(cancellationToken);

        // Get received requests
        var receivedRequests = await _context.RoommateRequests
            .Include(r => r.Requester)
            .Where(r => r.TargetUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new MyRequestDto(
                r.Id,
                r.RequesterId,
                r.Requester.FullName,
                r.Requester.Email,
                r.Message,
                r.Status.ToString(),
                r.CreatedAt,
                r.ProcessedAt
            ))
            .ToListAsync(cancellationToken);

        // Get active roommates
        var activeRoommates = await _context.RoommateRelationships
            .Include(r => r.User1)
            .Include(r => r.User2)
            .Where(r => r.IsActive && (r.User1Id == userId || r.User2Id == userId))
            .Select(r => new MyRoommateDto(
                r.Id,
                r.User1Id == userId ? r.User2Id : r.User1Id,
                r.User1Id == userId ? r.User2.FullName : r.User1.FullName,
                r.User1Id == userId ? r.User2.Email : r.User1.Email,
                r.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new GetMyRequestsResponse(sentRequests, receivedRequests, activeRoommates);
    }
}

