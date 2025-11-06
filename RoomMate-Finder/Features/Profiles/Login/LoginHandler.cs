using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Common;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Profiles.Login;

public class LoginHandler : IRequestHandler<LoginRequest, AuthResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly JwtService _jwtService;

    public LoginHandler(AppDbContext dbContext, JwtService jwtService)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Profiles
            .FirstOrDefaultAsync(p => p.Email == request.Email, cancellationToken);

        if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var token = _jwtService.GenerateToken(user);
        
        return new AuthResponse(
            user.Id,
            user.Email,
            user.FullName,
            token
        );
    }
}
