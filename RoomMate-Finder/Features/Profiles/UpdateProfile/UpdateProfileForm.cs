using Microsoft.AspNetCore.Http;

namespace RoomMate_Finder.Features.Profiles.UpdateProfile;

public class UpdateProfileForm
{
    public string? FullName { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? University { get; set; }
    public string? Bio { get; set; }
    public string? Lifestyle { get; set; }
    public string? Interests { get; set; }
    public IFormFile? ProfilePicture { get; set; }
}

