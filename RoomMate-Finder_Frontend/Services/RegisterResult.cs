namespace RoomMate_Finder_Frontend.Services;

public class RegisterResult
{
    public bool Successful { get; set; }
    public IEnumerable<string>? Errors { get; set; }
}
