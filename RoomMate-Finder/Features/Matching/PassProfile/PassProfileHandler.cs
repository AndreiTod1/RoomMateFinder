using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Matching.PassProfile;

public class PassProfileHandler : IRequestHandler<PassProfileRequest, PassProfileResponse>
{
    private readonly AppDbContext _dbContext;

    public PassProfileHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PassProfileResponse> Handle(PassProfileRequest request, CancellationToken cancellationToken)
    {
        // Validate that both users exist
        var usersExist = await ValidateUsersExist(request.UserId, request.TargetUserId, cancellationToken);
        if (!usersExist)
        {
            return new PassProfileResponse(false, "One or both users not found");
        }

        // Validate that user is not passing themselves
        if (request.UserId == request.TargetUserId)
        {
            return new PassProfileResponse(false, "Cannot pass yourself");
        }

        // Check if action already exists
        var existingAction = await CheckExistingAction(request.UserId, request.TargetUserId, cancellationToken);
        if (existingAction != null)
        {
            return new PassProfileResponse(false, $"You already {existingAction.ActionType.ToString().ToLower()}d this profile");
        }

        // Create the pass action
        var passAction = new UserAction
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            TargetUserId = request.TargetUserId,
            ActionType = ActionType.Pass,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.UserActions.Add(passAction);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PassProfileResponse(true, "Profile passed successfully");
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
}
