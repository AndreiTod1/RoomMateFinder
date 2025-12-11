using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Profiles;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Admins.GetAdmins;

public class GetAdminsHandler : IRequestHandler<GetAdminsRequest, List<ProfileResponse>>
{
    private readonly AppDbContext _dbContext;

    public GetAdminsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ProfileResponse>> Handle(GetAdminsRequest request, CancellationToken cancellationToken)
    {
        var admins = await _dbContext.Profiles
            .Where(p => p.Role == "Admin")
            .Select(p => new ProfileResponse(
                p.Id,
                p.Email,
                p.FullName,
                p.Age,
                p.Gender,
                p.University,
                p.Bio,
                p.Lifestyle,
                p.Interests,
                p.ProfilePicturePath,
                p.CreatedAt,
                p.Role
            ))
            .ToListAsync(cancellationToken);

        return admins;
    }
}
