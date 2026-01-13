using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using RoomMate_Finder.Common;

namespace RoomMate_Finder.Test.Common;

public class FileUploadHelperTests : IDisposable
{
    private readonly string _tempPath;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;

    public FileUploadHelperTests()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), "FileUploadTest_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempPath);
        
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(e => e.WebRootPath).Returns(_tempPath);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Given_EmptyUrl_When_DeleteProfilePictureIsCalled_Then_DoesNothing(string? url)
    {
        // Act
        // Should not throw
        FileUploadHelper.DeleteProfilePicture(url!, _mockEnvironment.Object);
    }

    [Fact]
    public void Given_ExistingFile_When_DeleteProfilePictureIsCalled_Then_DeletesFile()
    {
        // Arrange
        var fileName = "test.jpg";
        var fileUrl = "/" + fileName;
        var filePath = Path.Combine(_tempPath, fileName);
        File.WriteAllText(filePath, "dummy content");

        // Act
        FileUploadHelper.DeleteProfilePicture(fileUrl, _mockEnvironment.Object);

        // Assert
        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public void Given_NonExistentFile_When_DeleteProfilePictureIsCalled_Then_DoesNotThrow()
    {
        // Arrange
        var fileUrl = "/nonexistent.jpg";

        // Act
        // Should not throw
        FileUploadHelper.DeleteProfilePicture(fileUrl, _mockEnvironment.Object);
    }
    
    public void Dispose()
    {
        if (Directory.Exists(_tempPath))
        {
            try { Directory.Delete(_tempPath, true); } catch { }
        }
        GC.SuppressFinalize(this);
    }
}
