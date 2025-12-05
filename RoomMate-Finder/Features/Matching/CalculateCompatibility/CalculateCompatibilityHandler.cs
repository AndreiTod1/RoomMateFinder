using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Features.Matching.CalculateCompatibility.Services;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Matching.CalculateCompatibility;

public class CalculateCompatibilityHandler : IRequestHandler<CalculateCompatibilityRequest, CalculateCompatibilityResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly ICompatibilityCalculatorService _compatibilityCalculator;
    private readonly ICompatibilityDescriptionService _descriptionService;

    public CalculateCompatibilityHandler(
        AppDbContext dbContext,
        ICompatibilityCalculatorService compatibilityCalculator,
        ICompatibilityDescriptionService descriptionService)
    {
        _dbContext = dbContext;
        _compatibilityCalculator = compatibilityCalculator;
        _descriptionService = descriptionService;
    }

    public async Task<CalculateCompatibilityResponse> Handle(CalculateCompatibilityRequest request, CancellationToken cancellationToken)
    {
        var user1 = await GetUserProfile(request.UserId1, cancellationToken);
        var user2 = await GetUserProfile(request.UserId2, cancellationToken);

        var compatibilityResult = _compatibilityCalculator.CalculateCompatibility(user1, user2);
        var details = _descriptionService.CreateDetails(user1, user2, compatibilityResult);

        return new CalculateCompatibilityResponse(
            request.UserId1,
            request.UserId2,
            Math.Round(compatibilityResult.OverallScore, 2),
            compatibilityResult.CompatibilityLevel,
            details
        );
    }

    private async Task<Entities.Profile> GetUserProfile(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Profiles
            .FirstOrDefaultAsync(p => p.Id == userId, cancellationToken);

        if (user == null)
            throw new ArgumentException($"User with ID {userId} not found");

        return user;
    }
}
