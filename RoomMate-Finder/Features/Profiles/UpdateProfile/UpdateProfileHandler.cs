using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Common;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Profiles.UpdateProfile;

public class UpdateProfileHandler : IRequestHandler<UpdateProfileWithFileCommand, UpdateProfileResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public UpdateProfileHandler(AppDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }
    
    public async Task<UpdateProfileResponse> Handle(UpdateProfileWithFileCommand command, CancellationToken cancellationToken)
    {
        var request = command.Profile;

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

        // Handle profile picture update from multipart/form-data
        if (command.ProfilePicture is not null && command.ProfilePicture.Length > 0)
        {
            if (!string.IsNullOrWhiteSpace(profile.ProfilePicturePath))
            {
                FileUploadHelper.DeleteProfilePicture(profile.ProfilePicturePath, _environment);
            }

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
            profile.CreatedAt,
            profile.ProfilePicturePath
        );
    }
}