using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Profiles.GetProfiles;

public class GetProfilesHandler : IRequestHandler<GetProfilesRequest, List<GetProfilesResponse>>
{
    private readonly AppDbContext _dbContext;

    public GetProfilesHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<GetProfilesResponse>> Handle(GetProfilesRequest request, CancellationToken cancellationToken)
    {
        var profiles = await _dbContext.Profiles
            .Select(p => new GetProfilesResponse(
                p.Id,
                p.Email,
                p.FullName,
                p.Age,
                p.Gender,
                p.University,
                p.Bio,
                p.Lifestyle,
                p.Interests,
                p.CreatedAt,
                p.ProfilePicturePath
            ))
            .ToListAsync(cancellationToken);

        return profiles;
    }
}