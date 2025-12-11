using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Features.Matching.CalculateCompatibility.Services;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Matching.GetMatches;

public class GetMatchesHandler : IRequestHandler<GetMatchesRequest, List<GetMatchesResponse>>
{
    private readonly AppDbContext _dbContext;
    private readonly ICompatibilityCalculatorService _compatibilityCalculator;

    public GetMatchesHandler(
        AppDbContext dbContext,
        ICompatibilityCalculatorService compatibilityCalculator)
    {
        _dbContext = dbContext;
        _compatibilityCalculator = compatibilityCalculator;
    }

    public async Task<List<GetMatchesResponse>> Handle(GetMatchesRequest request, CancellationToken cancellationToken)
    {
        // Get current user profile
        var currentUser = await GetUserProfile(request.UserId, cancellationToken);

        // Get all other profiles (excluding current user)
        var otherProfiles = await GetOtherProfiles(request.UserId, cancellationToken);

        // Calculate compatibility for each profile and create response
        var matches = CalculateMatchesWithCompatibility(currentUser, otherProfiles);

        // Sort by compatibility score descending
        return matches.OrderByDescending(m => m.CompatibilityScore).ToList();
    }

    private async Task<Entities.Profile> GetUserProfile(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Profiles
            .FirstOrDefaultAsync(p => p.Id == userId, cancellationToken);

        if (user == null)
            throw new ArgumentException($"User with ID {userId} not found");

        return user;
    }

    private async Task<List<Entities.Profile>> GetOtherProfiles(Guid currentUserId, CancellationToken cancellationToken)
    {
        // Get IDs of users that current user has already liked or passed
        var actedUponUserIds = await _dbContext.UserActions
            .Where(ua => ua.UserId == currentUserId)
            .Select(ua => ua.TargetUserId)
            .ToListAsync(cancellationToken);

        // Return profiles excluding current user and users already acted upon
        return await _dbContext.Profiles
            .Where(p => p.Id != currentUserId && !actedUponUserIds.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    private List<GetMatchesResponse> CalculateMatchesWithCompatibility(
        Entities.Profile currentUser, 
        List<Entities.Profile> otherProfiles)
    {
        var matches = new List<GetMatchesResponse>();

        foreach (var profile in otherProfiles)
        {
            var compatibilityResult = _compatibilityCalculator.CalculateCompatibility(currentUser, profile);

            var match = new GetMatchesResponse(
                profile.Id,
                profile.Email,
                profile.FullName,
                profile.Age,
                profile.Gender,
                profile.University,
                profile.Bio,
                profile.Lifestyle,
                profile.Interests,
                Math.Round(compatibilityResult.OverallScore, 2),
                compatibilityResult.CompatibilityLevel,
                profile.CreatedAt,
                profile.ProfilePicturePath // new mapping so Discover can show images
            );

            matches.Add(match);
        }

        return matches;
    }
}
