using FluentAssertions;
using RoomMate_Finder_Frontend.Shared;
using System.Text;
using Xunit;

namespace RoomMate_Finder_Frontend.Test.Shared;

public class CroppedBrowserFileTests
{
    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        var data = Encoding.UTF8.GetBytes("test");
        var name = "test.txt";
        var type = "text/plain";

        var file = new CroppedBrowserFile(data, name, type);

        file.Name.Should().Be(name);
        file.ContentType.Should().Be(type);
        file.Size.Should().Be(data.Length);
        file.LastModified.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void OpenReadStream_ReturnsCorrectData()
    {
        var content = "Hello World";
        var data = Encoding.UTF8.GetBytes(content);
        var file = new CroppedBrowserFile(data, "test.txt", "text/plain");

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        var result = reader.ReadToEnd();

        result.Should().Be(content);
    }

    [Fact]
    public void OpenReadStream_ThrowsIfSizeExceedsMax()
    {
        var data = new byte[1024];
        var file = new CroppedBrowserFile(data, "test.txt", "text/plain");

        var act = () => file.OpenReadStream(maxAllowedSize: 500);

        act.Should().Throw<IOException>()
           .WithMessage("*exceeds the maximum allowed size*");
    }
}
