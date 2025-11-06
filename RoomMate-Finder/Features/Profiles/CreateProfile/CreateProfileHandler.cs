using MediatR;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Profiles;

public class CreateProfileHandler : IRequestHandler<CreateProfileRequest, Guid>
{
    private readonly AppDbContext _dbContext;

    public CreateProfileHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Age = request.Age,
            Gender = request.Gender,
            University = request.University,
            Bio = request.Bio,
            Lifestyle = request.Lifestyle,
            Interests = request.Interests,
            CreatedAt = DateTime.UtcNow
        };
        
        _dbContext.Profiles.Add(profile);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return profile.Id;
    }
    
}