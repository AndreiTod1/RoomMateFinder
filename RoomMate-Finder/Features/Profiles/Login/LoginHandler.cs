using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Common;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Profiles.Login;

public class LoginHandler : IRequestHandler<LoginRequest, AuthResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly JwtService _jwtService;
    private readonly IValidator<LoginRequest> _validator;

    public LoginHandler(AppDbContext dbContext, JwtService jwtService, IValidator<LoginRequest> validator)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _validator = validator;
    }

    public async Task<AuthResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new UnauthorizedAccessException(errors);
        }

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
