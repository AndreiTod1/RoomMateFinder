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
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (profile == null)
        {
            return null;
        }

        return new GetProfileByIdResponse(
            profile.Id,
            profile.Email,
            profile.FullName,
            profile.Age,
            profile.Gender,
            profile.University,
            profile.Bio,
            profile.Lifestyle,
            profile.Interests,
            profile.CreatedAt
        );
    }
}