using System.Threading.Tasks;

namespace RoomMate_Finder_Frontend.Services;

public interface IAuthService
{
    Task LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    Task<RegisterResult> RegisterWithPictureAsync(string email, string password, string fullName, int age, string gender, 
        string university, string bio, string lifestyle, string interests, string? profilePictureUrl);
}

