using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Infrastructure.Persistence;
using System.Security.Claims;

namespace RoomMate_Finder.Features.Roommates.SendRequest;

public class SendRoommateRequestHandler : IRequestHandler<SendRoommateRequestRequest, SendRoommateRequestResponse>
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SendRoommateRequestHandler(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<SendRoommateRequestResponse> Handle(SendRoommateRequestRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var requesterId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        // Check if target user exists
        var targetUser = await _context.Profiles
            .FirstOrDefaultAsync(p => p.Id == request.TargetUserId, cancellationToken);
        
        if (targetUser == null)
        {
            throw new InvalidOperationException("Target user not found");
        }

        // Cannot send request to yourself
        if (requesterId == request.TargetUserId)
        {
            throw new InvalidOperationException("Cannot send a roommate request to yourself");
        }

        // Check for existing pending or mutually confirmed request from current user
        var existingRequest = await _context.RoommateRequests
            .FirstOrDefaultAsync(r => 
                r.RequesterId == requesterId && 
                r.TargetUserId == request.TargetUserId && 
                (r.Status == RoommateRequestStatus.Pending || r.Status == RoommateRequestStatus.MutuallyConfirmed), 
                cancellationToken);
        
        if (existingRequest != null)
        {
            throw new InvalidOperationException("You already have a pending request to this user");
        }

        // Check if there's already an active relationship between these users
        var existingRelationship = await _context.RoommateRelationships
            .FirstOrDefaultAsync(r => 
                r.IsActive && 
                ((r.User1Id == requesterId && r.User2Id == request.TargetUserId) ||
                 (r.User1Id == request.TargetUserId && r.User2Id == requesterId)), 
                cancellationToken);
        
        if (existingRelationship != null)
        {
            throw new InvalidOperationException("You already have an active roommate relationship with this user");
        }

        // Check if the other user has already sent a request to current user (inverse request)
        var inverseRequest = await _context.RoommateRequests
            .FirstOrDefaultAsync(r => 
                r.RequesterId == request.TargetUserId && 
                r.TargetUserId == requesterId && 
                r.Status == RoommateRequestStatus.Pending, 
                cancellationToken);

        var roommateRequest = new RoommateRequest
        {
            Id = Guid.NewGuid(),
            RequesterId = requesterId,
            TargetUserId = request.TargetUserId,
            Message = request.Message,
            Status = RoommateRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        string responseMessage;

        if (inverseRequest != null)
        {
            // Both users have now sent requests - update both to MutuallyConfirmed
            inverseRequest.Status = RoommateRequestStatus.MutuallyConfirmed;
            roommateRequest.Status = RoommateRequestStatus.MutuallyConfirmed;
            responseMessage = "Both users have confirmed! Your request is now waiting for admin approval.";
        }
        else
        {
            responseMessage = "Roommate request sent successfully. Waiting for the other user to confirm.";
        }

        _context.RoommateRequests.Add(roommateRequest);
        await _context.SaveChangesAsync(cancellationToken);

        return new SendRoommateRequestResponse(roommateRequest.Id, responseMessage);
    }
}

