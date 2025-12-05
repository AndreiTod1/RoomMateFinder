using Microsoft.AspNetCore.Http;

namespace RoomMate_Finder.Features.Profiles.CreateProfile;

public class CreateProfileForm
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string University { get; set; } = string.Empty;
    public string Lifestyle { get; set; } = string.Empty;
    public string Interests { get; set; } = string.Empty;
    public IFormFile? ProfilePicture { get; set; }
}

