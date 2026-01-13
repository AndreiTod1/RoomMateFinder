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
            .FirstOrDefaultAsync(p => p.Id == request.UserId, cancellationToken) 
            ?? throw new KeyNotFoundException("Profile not found");
        
        UpdateProfileFields(profile, request);
        await UpdateProfilePictureAsync(profile, command, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreateResponse(profile);
    }

    private static void UpdateProfileFields(Entities.Profile profile, UpdateProfileRequest request)
    {
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
    }

    private async Task UpdateProfilePictureAsync(Entities.Profile profile, UpdateProfileWithFileCommand command, CancellationToken cancellationToken)
    {
        if (command.ProfilePicture is null || command.ProfilePicture.Length == 0)
            return;

        if (!string.IsNullOrWhiteSpace(profile.ProfilePicturePath))
        {
            FileUploadHelper.DeleteProfilePicture(profile.ProfilePicturePath, _environment);
        }

        var extension = GetFileExtension(command.ProfilePicture);
        var uploadsPath = EnsureUploadsDirectory();
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

    private static string GetFileExtension(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!string.IsNullOrEmpty(extension))
            return extension;

        return file.ContentType switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/webp" => ".webp",
            _ => ".jpg"
        };
    }

    private string EnsureUploadsDirectory()
    {
        var uploadsPath = Path.Combine(_environment.WebRootPath, "profile-pictures");
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
        }
        return uploadsPath;
    }

    private static UpdateProfileResponse CreateResponse(Entities.Profile profile)
    {
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