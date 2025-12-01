using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Matching.GetUserMatches;

public class GetUserMatchesHandler : IRequestHandler<GetUserMatchesRequest, List<GetUserMatchesResponse>>
{
    private readonly AppDbContext _dbContext;

    public GetUserMatchesHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<GetUserMatchesResponse>> Handle(GetUserMatchesRequest request, CancellationToken cancellationToken)
    {
        var matches = await _dbContext.Matches
            .Include(m => m.User1)
            .Include(m => m.User2)
            .Where(m => (m.User1Id == request.UserId || m.User2Id == request.UserId) && m.IsActive)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        var response = new List<GetUserMatchesResponse>();

        foreach (var match in matches)
        {
            // Determine which user is the matched user (not the current user)
            var matchedUser = match.User1Id == request.UserId ? match.User2 : match.User1;

            response.Add(new GetUserMatchesResponse(
                match.Id,
                matchedUser.Id,
                matchedUser.Email,
                matchedUser.FullName,
                matchedUser.Age,
                matchedUser.Gender,
                matchedUser.University,
                matchedUser.Bio,
                matchedUser.Lifestyle,
                matchedUser.Interests,
                match.CreatedAt,
                match.IsActive
            ));
        }

        return response;
    }
}
