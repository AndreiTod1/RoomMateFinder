using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Infrastructure.Persistence;
using System.Security.Claims;

namespace RoomMate_Finder.Features.Roommates.RejectRequest;

public class RejectRequestHandler : IRequestHandler<RejectRequestRequest, RejectRequestResponse>
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RejectRequestHandler(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<RejectRequestResponse> Handle(RejectRequestRequest request, CancellationToken cancellationToken)
    {
        var adminIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
        {
            throw new UnauthorizedAccessException("Admin not authenticated");
        }

        var roommateRequest = await _context.RoommateRequests
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken);

        if (roommateRequest == null)
        {
            throw new InvalidOperationException("Request not found");
        }

        if (roommateRequest.Status != RoommateRequestStatus.Pending)
        {
            throw new InvalidOperationException("This request has already been processed");
        }

        // Update request status
        roommateRequest.Status = RoommateRequestStatus.Rejected;
        roommateRequest.ProcessedAt = DateTime.UtcNow;
        roommateRequest.ProcessedByAdminId = adminId;

        await _context.SaveChangesAsync(cancellationToken);

        return new RejectRequestResponse("Roommate request has been rejected");
    }
}

