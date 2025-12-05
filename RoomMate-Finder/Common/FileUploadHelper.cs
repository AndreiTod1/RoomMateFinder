using Microsoft.AspNetCore.Mvc;

namespace RoomMate_Finder.Common;

public static class FileUploadHelper
{
    public static void DeleteProfilePicture(string pictureUrl, IWebHostEnvironment environment)
    {
        if (string.IsNullOrWhiteSpace(pictureUrl)) return;

        try
        {
            var filePath = Path.Combine(environment.WebRootPath, pictureUrl.TrimStart('/'));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
            // Ignore deletion errors
        }
    }
}
