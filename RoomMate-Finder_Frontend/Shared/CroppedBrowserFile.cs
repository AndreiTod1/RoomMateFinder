using Microsoft.AspNetCore.Components.Forms;

namespace RoomMate_Finder_Frontend.Shared;

/// <summary>
/// Custom IBrowserFile implementation for cropped image data
/// </summary>
public class CroppedBrowserFile : IBrowserFile
{
    private readonly byte[] _data;
    
    public CroppedBrowserFile(byte[] data, string name, string contentType)
    {
        _data = data;
        Name = name;
        ContentType = contentType;
        Size = data.Length;
        LastModified = DateTimeOffset.UtcNow;
    }

    public string Name { get; }
    public DateTimeOffset LastModified { get; }
    public long Size { get; }
    public string ContentType { get; }

    public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
    {
        if (Size > maxAllowedSize)
        {
            throw new IOException($"The file size ({Size} bytes) exceeds the maximum allowed size ({maxAllowedSize} bytes).");
        }

        return new MemoryStream(_data, writable: false);
    }
}

