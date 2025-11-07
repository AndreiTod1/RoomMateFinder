using System.Threading.Tasks;

namespace RoomMate_Finder_Frontend.Services;

public interface IAuthService
{
    Task LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
}

