using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Common;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.Profiles.Login;
using RoomMate_Finder.Infrastructure.Persistence;
using RoomMate_Finder.Features.Profiles.CreateProfile;

namespace RoomMate_Finder.Features.Profiles;

public class CreateProfileHandler : IRequestHandler<CreateProfileWithFileCommand, AuthResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly JwtService _jwtService;
    private readonly IValidator<CreateProfileRequest> _validator;
    private readonly IWebHostEnvironment _environment;

    public CreateProfileHandler(AppDbContext dbContext, JwtService jwtService, IValidator<CreateProfileRequest> validator, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _validator = validator;
        _environment = environment;
    }

    public async Task<AuthResponse> Handle(CreateProfileWithFileCommand command, CancellationToken cancellationToken)
    {
        var request = command.Profile;

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new InvalidOperationException(errors);
        }

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
        
        // Save profile picture from multipart/form-data if provided
        if (command.ProfilePicture is not null && command.ProfilePicture.Length > 0)
        {
            var extension = Path.GetExtension(command.ProfilePicture.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension))
            {
                extension = command.ProfilePicture.ContentType switch
                {
                    "image/png" => ".png",
                    "image/jpeg" => ".jpg",
                    "image/webp" => ".webp",
                    _ => ".jpg"
                };
            }

            var uploadsPath = Path.Combine(_environment.WebRootPath, "profile-pictures");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var fileName = $"{profile.Id}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await command.ProfilePicture.CopyToAsync(stream, cancellationToken);
            }

            profile.ProfilePicturePath = $"/profile-pictures/{fileName}";
        }
        
        _dbContext.Profiles.Add(profile);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        var token = _jwtService.GenerateToken(profile);
        return new AuthResponse(profile.Id, profile.Email, profile.FullName, token, profile.ProfilePicturePath);
    }
}