using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Matching.LikeProfile;

public class LikeProfileHandler : IRequestHandler<LikeProfileRequest, LikeProfileResponse>
{
    private readonly AppDbContext _dbContext;

    public LikeProfileHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LikeProfileResponse> Handle(LikeProfileRequest request, CancellationToken cancellationToken)
    {
        // Validate that both users exist
        var usersExist = await ValidateUsersExist(request.UserId, request.TargetUserId, cancellationToken);
        if (!usersExist)
        {
            return new LikeProfileResponse(false, "One or both users not found");
        }

        // Validate that user is not liking themselves
        if (request.UserId == request.TargetUserId)
        {
            return new LikeProfileResponse(false, "Cannot like yourself");
        }

        // Check if action already exists
        var existingAction = await CheckExistingAction(request.UserId, request.TargetUserId, cancellationToken);
        if (existingAction != null)
        {
            return new LikeProfileResponse(false, $"You already {existingAction.ActionType.ToString().ToLower()}d this profile");
        }

        // Create the like action
        var likeAction = new UserAction
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            TargetUserId = request.TargetUserId,
            ActionType = ActionType.Like,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.UserActions.Add(likeAction);

        // Check if there's a mutual like (match)
        var mutualLike = await CheckMutualLike(request.UserId, request.TargetUserId, cancellationToken);
        
        if (mutualLike)
        {
            // Create a match
            var match = await CreateMatch(request.UserId, request.TargetUserId, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            
            return new LikeProfileResponse(
                true, 
                "It's a match! You both liked each other!", 
                true, 
                match.Id
            );
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new LikeProfileResponse(true, "Profile liked successfully");
    }

    private async Task<bool> ValidateUsersExist(Guid userId, Guid targetUserId, CancellationToken cancellationToken)
    {
        var userCount = await _dbContext.Profiles
            .CountAsync(p => p.Id == userId || p.Id == targetUserId, cancellationToken);
        return userCount == 2;
    }

    private async Task<UserAction?> CheckExistingAction(Guid userId, Guid targetUserId, CancellationToken cancellationToken)
    {
        return await _dbContext.UserActions
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.TargetUserId == targetUserId, cancellationToken);
    }

    private async Task<bool> CheckMutualLike(Guid userId, Guid targetUserId, CancellationToken cancellationToken)
    {
        // Check if the target user has already liked the current user
        return await _dbContext.UserActions
            .AnyAsync(ua => ua.UserId == targetUserId && 
                           ua.TargetUserId == userId && 
                           ua.ActionType == ActionType.Like, cancellationToken);
    }

    private async Task<Match> CreateMatch(Guid userId, Guid targetUserId, CancellationToken cancellationToken)
    {
        // Ensure consistent ordering to avoid duplicate matches
        var user1Id = userId < targetUserId ? userId : targetUserId;
        var user2Id = userId < targetUserId ? targetUserId : userId;

        // Check if match already exists
        var existingMatch = await _dbContext.Matches
            .FirstOrDefaultAsync(m => m.User1Id == user1Id && m.User2Id == user2Id, cancellationToken);

        if (existingMatch != null)
        {
            return existingMatch;
        }

        var match = new Match
        {
            Id = Guid.NewGuid(),
            User1Id = user1Id,
            User2Id = user2Id,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _dbContext.Matches.Add(match);
        return match;
    }
}
