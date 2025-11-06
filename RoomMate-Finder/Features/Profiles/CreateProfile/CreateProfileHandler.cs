using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Common;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Profiles.Login;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Profiles;

public class CreateProfileHandler : IRequestHandler<CreateProfileRequest, AuthResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly JwtService _jwtService;

    public CreateProfileHandler(AppDbContext dbContext, JwtService jwtService)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> Handle(CreateProfileRequest request, CancellationToken cancellationToken)
    {
        // Verifică dacă emailul există deja
        var existingUser = await _dbContext.Profiles
            .FirstOrDefaultAsync(p => p.Email == request.Email, cancellationToken);
            
        if (existingUser != null)
        {
            throw new InvalidOperationException("Email already registered");
        }

        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = PasswordHasher.HashPassword(request.Password),
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
        
        var token = _jwtService.GenerateToken(profile);
        return new AuthResponse(profile.Id, profile.Email, profile.FullName, token);
    }
}