using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Infrastructure.Persistence;
using System.Security.Claims;

namespace RoomMate_Finder.Features.Roommates.ApproveRequest;

public class ApproveRequestHandler : IRequestHandler<ApproveRequestRequest, ApproveRequestResponse>
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApproveRequestHandler(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApproveRequestResponse> Handle(ApproveRequestRequest request, CancellationToken cancellationToken)
    {
        var adminIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
        {
            throw new UnauthorizedAccessException("Admin not authenticated");
        }

        var roommateRequest = await _context.RoommateRequests
            .Include(r => r.Requester)
            .Include(r => r.TargetUser)
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken);

        if (roommateRequest == null)
        {
            throw new InvalidOperationException("Request not found");
        }

        if (roommateRequest.Status != RoommateRequestStatus.Pending)
        {
            throw new InvalidOperationException("This request has already been processed");
        }

        var existingRelationship = await _context.RoommateRelationships
            .FirstOrDefaultAsync(r => 
                r.IsActive && 
                ((r.User1Id == roommateRequest.RequesterId && r.User2Id == roommateRequest.TargetUserId) ||
                 (r.User1Id == roommateRequest.TargetUserId && r.User2Id == roommateRequest.RequesterId)), 
                cancellationToken);

        if (existingRelationship != null)
        {
            throw new InvalidOperationException("An active relationship already exists between these users");
        }

        roommateRequest.Status = RoommateRequestStatus.Approved;
        roommateRequest.ProcessedAt = DateTime.UtcNow;
        roommateRequest.ProcessedByAdminId = adminId;

        var relationship = new RoommateRelationship
        {
            Id = Guid.NewGuid(),
            User1Id = roommateRequest.RequesterId,
            User2Id = roommateRequest.TargetUserId,
            ApprovedByAdminId = adminId,
            OriginalRequestId = roommateRequest.Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.RoommateRelationships.Add(relationship);
        await _context.SaveChangesAsync(cancellationToken);

        return new ApproveRequestResponse(
            relationship.Id, 
            $"Roommate relationship approved between {roommateRequest.Requester.FullName} and {roommateRequest.TargetUser.FullName}"
        );
    }
}

