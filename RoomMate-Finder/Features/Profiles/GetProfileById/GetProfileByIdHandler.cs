using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Profiles.GetProfileById;

public class GetProfileByIdHandler : IRequestHandler<GetProfileByIdRequest, GetProfileByIdResponse?>
{
    private readonly AppDbContext _dbContext;

    public GetProfileByIdHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetProfileByIdResponse?> Handle(GetProfileByIdRequest request, CancellationToken cancellationToken)
    {
        var profile = await _dbContext.Profiles
            .AsNoTracking()
            .Where(p => p.Id == request.Id)
            .Select(p => new GetProfileByIdResponse(
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
            .SingleOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            throw new InvalidOperationException("Profile not found");
        }

        return profile;
    }
}