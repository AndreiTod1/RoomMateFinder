using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Profiles.UpdateProfile;

public class UpdateProfileHandler : IRequestHandler<UpdateProfileRequest, UpdateProfileResponse>
{
    private readonly AppDbContext _dbContext;

    public UpdateProfileHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<UpdateProfileResponse> Handle(UpdateProfileRequest request, CancellationToken cancellationToken)
    {

        var profile = await _dbContext.Profiles
            .FirstOrDefaultAsync(p => p.Id == request.UserId, cancellationToken);
            
        if (profile == null)
        {
            throw new KeyNotFoundException("Profile not found");
        }
        
        if (request.FullName != null)
            profile.FullName = request.FullName;
            
        if (request.Age.HasValue)
            profile.Age = request.Age.Value;
            
        if (request.Gender != null)
            profile.Gender = request.Gender;
            
        if (request.University != null)
            profile.University = request.University;
            
        if (request.Bio != null)
            profile.Bio = request.Bio;
            
        if (request.Lifestyle != null)
            profile.Lifestyle = request.Lifestyle;
            
        if (request.Interests != null)
            profile.Interests = request.Interests;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateProfileResponse(
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