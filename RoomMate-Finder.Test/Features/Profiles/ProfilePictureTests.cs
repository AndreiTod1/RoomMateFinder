using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using RoomMate_Finder.Features.Profiles;
using RoomMate_Finder.Features.Profiles.CreateProfile;
using RoomMate_Finder.Features.Profiles.UpdateProfile;
using RoomMate_Finder.Common;
using RoomMate_Finder.Infrastructure.Persistence;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Validators;
using FluentValidation;
using System.Text;

namespace RoomMate_Finder.Test.Features.Profiles;

public class ProfilePictureTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly JwtService _jwtService;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly string _tempPath;

    public ProfilePictureTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "RoomMateFinderTest_" + Guid.NewGuid())
            .Options;
        
        _dbContext = new AppDbContext(options);
        _jwtService = new JwtService(
            "test-jwt-key-that-is-at-least-32-characters-long-for-security",
            "TestIssuer",
            "TestAudience"
        );
        
        // Create temp directory for file uploads
        _tempPath = Path.Combine(Path.GetTempPath(), "RoomMateFinderTest_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempPath);
        Directory.CreateDirectory(Path.Combine(_tempPath, "profile-pictures"));
        
        // Mock IWebHostEnvironment
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(e => e.WebRootPath).Returns(_tempPath);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        // Clean up temp directory
        if (Directory.Exists(_tempPath))
        {
            try { Directory.Delete(_tempPath, true); } catch { }
        }
        GC.SuppressFinalize(this);
    }

    private static IFormFile CreateMockFormFile(string fileName, string contentType, byte[] content)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "ProfilePicture", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    [Fact]
    public async Task CreateProfile_WithProfilePicture_ShouldStoreAndReturn()
    {
        // Arrange
        var request = new CreateProfileRequest(
            Email: "test@example.com",
            Password: "TestPass123!",
            FullName: "Test User",
            Bio: "Test bio",
            Age: 25,
            Gender: "Male",
            University: "Test University",
            Lifestyle: "Active",
            Interests: "Sports"
        );

        var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", Encoding.UTF8.GetBytes("fake image data"));
        var command = new CreateProfileWithFileCommand(request, mockFile);

        var validator = new Mock<IValidator<CreateProfileRequest>>();
        validator.Setup(x => x.ValidateAsync(It.IsAny<CreateProfileRequest>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var handler = new CreateProfileHandler(_dbContext, _jwtService, validator.Object, _mockEnvironment.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("test@example.com", response.Email);
        Assert.NotNull(response.ProfilePicturePath);
        Assert.StartsWith("/profile-pictures/", response.ProfilePicturePath);
        
        var savedProfile = await _dbContext.Profiles.FirstOrDefaultAsync(p => p.Id == response.UserId);
        Assert.NotNull(savedProfile);
        Assert.StartsWith("/profile-pictures/", savedProfile.ProfilePicturePath);
    }

    [Fact]
    public async Task CreateProfile_WithoutProfilePicture_ShouldStoreNullPicture()
    {
        // Arrange
        var request = new CreateProfileRequest(
            Email: "test2@example.com",
            Password: "TestPass123!",
            FullName: "Test User 2",
            Bio: "Test bio",
            Age: 25,
            Gender: "Female",
            University: "Test University",
            Lifestyle: "Calm",
            Interests: "Reading"
        );

        var command = new CreateProfileWithFileCommand(request, null);

        var validator = new Mock<IValidator<CreateProfileRequest>>();
        validator.Setup(x => x.ValidateAsync(It.IsAny<CreateProfileRequest>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var handler = new CreateProfileHandler(_dbContext, _jwtService, validator.Object, _mockEnvironment.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.ProfilePicturePath);
    }

    [Fact]
    public async Task UpdateProfile_WithProfilePicture_ShouldUpdatePicture()
    {
        // Arrange - Create initial profile
        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "test3@example.com",
            PasswordHash = PasswordHasher.HashPassword("TestPass123!"),
            FullName = "Test User 3",
            Age = 25,
            Gender = "Male",
            University = "Test University",
            Bio = "Original bio",
            Lifestyle = "Active",
            Interests = "Gaming",
            ProfilePicturePath = null
        };
        
        _dbContext.Profiles.Add(profile);
        await _dbContext.SaveChangesAsync();

        var mockFile = CreateMockFormFile("new.png", "image/png", Encoding.UTF8.GetBytes("new fake image"));
        var updateRequest = new UpdateProfileRequest(
            FullName: null,
            Age: null,
            Gender: null,
            University: null,
            Bio: null,
            Lifestyle: null,
            Interests: null
        )
        {
            UserId = profile.Id
        };

        var command = new UpdateProfileWithFileCommand(updateRequest, mockFile);
        var handler = new UpdateProfileHandler(_dbContext, _mockEnvironment.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.ProfilePicturePath);
        Assert.StartsWith("/profile-pictures/", response.ProfilePicturePath);
        
        var updatedProfile = await _dbContext.Profiles.FirstOrDefaultAsync(p => p.Id == profile.Id);
        Assert.NotNull(updatedProfile);
        Assert.StartsWith("/profile-pictures/", updatedProfile.ProfilePicturePath);
    }

    [Fact]
    public async Task UpdateProfile_WithoutProfilePicture_ShouldKeepExistingPicture()
    {
        // Arrange - Create initial profile with picture
        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "test4@example.com",
            PasswordHash = PasswordHasher.HashPassword("TestPass123!"),
            FullName = "Test User 4",
            Age = 25,
            Gender = "Male",
            University = "Test University",
            Bio = "Original bio",
            Lifestyle = "Active",
            Interests = "Gaming",
            ProfilePicturePath = "/profile-pictures/existing.jpg"
        };
        
        _dbContext.Profiles.Add(profile);
        await _dbContext.SaveChangesAsync();

        var updateRequest = new UpdateProfileRequest(
            FullName: "Updated Name",
            Age: 26,
            Gender: null,
            University: null,
            Bio: "Updated bio",
            Lifestyle: null,
            Interests: null
        )
        {
            UserId = profile.Id
        };

        var command = new UpdateProfileWithFileCommand(updateRequest, null);
        var handler = new UpdateProfileHandler(_dbContext, _mockEnvironment.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Updated Name", response.FullName);
        Assert.Equal(26, response.Age);
        Assert.Equal("/profile-pictures/existing.jpg", response.ProfilePicturePath);
    }

    [Fact]
    public void CreateProfileValidator_WithValidData_ShouldPass()
    {
        // Arrange
        var validator = new CreateProfileValidator();
        var request = new CreateProfileRequest(
            Email: "test5@example.com",
            Password: "TestPass123!",
            FullName: "Test User 5",
            Bio: "Test bio",
            Age: 25,
            Gender: "Male",
            University: "Test University",
            Lifestyle: "Active",
            Interests: "Music"
        );

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateProfileValidator_WithInvalidEmail_ShouldFail()
    {
        // Arrange
        var validator = new CreateProfileValidator();
        var request = new CreateProfileRequest(
            Email: "invalid-email",
            Password: "TestPass123!",
            FullName: "Test User",
            Bio: "Test bio",
            Age: 25,
            Gender: "Male",
            University: "Test University",
            Lifestyle: "Active",
            Interests: "Art"
        );

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateProfileValidator_WithValidData_ShouldPass()
    {
        // Arrange
        var validator = new UpdateProfileValidator();
        var request = new UpdateProfileRequest(
            FullName: "Updated Name",
            Age: 26,
            Gender: "Female",
            University: "New University",
            Bio: "Updated bio",
            Lifestyle: "Calm",
            Interests: "Reading"
        );

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateProfileValidator_WithNullValues_ShouldPass()
    {
        // Arrange
        var validator = new UpdateProfileValidator();
        var request = new UpdateProfileRequest(
            FullName: null,
            Age: null,
            Gender: null,
            University: null,
            Bio: null,
            Lifestyle: null,
            Interests: null
        );

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    #region Profile Picture Crop Tests

    [Fact]
    public async Task CreateProfile_WithJpegImage_ShouldSaveWithJpgExtension()
    {
        // Arrange
        var request = new CreateProfileRequest(
            Email: "jpeg-test@example.com",
            Password: "TestPass123!",
            FullName: "JPEG Test User",
            Bio: "Test bio",
            Age: 25,
            Gender: "Male",
            University: "Test University",
            Lifestyle: "Active",
            Interests: "Photography"
        );

        var mockFile = CreateMockFormFile("photo.jpeg", "image/jpeg", CreateFakeImageBytes());
        var command = new CreateProfileWithFileCommand(request, mockFile);

        var validator = new Mock<IValidator<CreateProfileRequest>>();
        validator.Setup(x => x.ValidateAsync(It.IsAny<CreateProfileRequest>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var handler = new CreateProfileHandler(_dbContext, _jwtService, validator.Object, _mockEnvironment.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response.ProfilePicturePath);
        Assert.EndsWith(".jpeg", response.ProfilePicturePath);
    }

    [Fact]
    public async Task CreateProfile_WithPngImage_ShouldSaveWithPngExtension()
    {
        // Arrange
        var request = new CreateProfileRequest(
            Email: "png-test@example.com",
            Password: "TestPass123!",
            FullName: "PNG Test User",
            Bio: "Test bio",
            Age: 25,
            Gender: "Female",
            University: "Test University",
            Lifestyle: "Calm",
            Interests: "Art"
        );

        var mockFile = CreateMockFormFile("avatar.png", "image/png", CreateFakeImageBytes());
        var command = new CreateProfileWithFileCommand(request, mockFile);

        var validator = new Mock<IValidator<CreateProfileRequest>>();
        validator.Setup(x => x.ValidateAsync(It.IsAny<CreateProfileRequest>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var handler = new CreateProfileHandler(_dbContext, _jwtService, validator.Object, _mockEnvironment.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response.ProfilePicturePath);
        Assert.EndsWith(".png", response.ProfilePicturePath);
    }

    [Fact]
    public async Task CreateProfile_WithWebpImage_ShouldSaveWithWebpExtension()
    {
        // Arrange
        var request = new CreateProfileRequest(
            Email: "webp-test@example.com",
            Password: "TestPass123!",
            FullName: "WebP Test User",
            Bio: "Test bio",
            Age: 30,
            Gender: "Other",
            University: "Tech University",
            Lifestyle: "Active",
            Interests: "Technology"
        );

        var mockFile = CreateMockFormFile("image.webp", "image/webp", CreateFakeImageBytes());
        var command = new CreateProfileWithFileCommand(request, mockFile);

        var validator = new Mock<IValidator<CreateProfileRequest>>();
        validator.Setup(x => x.ValidateAsync(It.IsAny<CreateProfileRequest>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var handler = new CreateProfileHandler(_dbContext, _jwtService, validator.Object, _mockEnvironment.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response.ProfilePicturePath);
        Assert.EndsWith(".webp", response.ProfilePicturePath);
    }

    [Fact]
    public async Task UpdateProfile_WithNewPicture_ShouldReplaceExistingPicture()
    {
        // Arrange - Create profile with existing picture
        var profileId = Guid.NewGuid();
        var oldFileName = $"{profileId}.jpg";
        var oldFilePath = Path.Combine(_tempPath, "profile-pictures", oldFileName);
        
        // Create the old file
        await File.WriteAllBytesAsync(oldFilePath, Encoding.UTF8.GetBytes("old image data"));
        
        var profile = new Profile
        {
            Id = profileId,
            Email = "replace-test@example.com",
            PasswordHash = PasswordHasher.HashPassword("TestPass123!"),
            FullName = "Replace Test User",
            Age = 28,
            Gender = "Male",
            University = "Test University",
            Bio = "Test bio",
            Lifestyle = "Active",
            Interests = "Sports",
            ProfilePicturePath = $"/profile-pictures/{oldFileName}"
        };
        
        _dbContext.Profiles.Add(profile);
        await _dbContext.SaveChangesAsync();

        // Create new image file
        var mockFile = CreateMockFormFile("new-photo.png", "image/png", CreateFakeImageBytes());
        var updateRequest = new UpdateProfileRequest(
            FullName: null,
            Age: null,
            Gender: null,
            University: null,
            Bio: null,
            Lifestyle: null,
            Interests: null
        )
        {
            UserId = profileId
        };

        var command = new UpdateProfileWithFileCommand(updateRequest, mockFile);
        var handler = new UpdateProfileHandler(_dbContext, _mockEnvironment.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response.ProfilePicturePath);
        Assert.EndsWith(".png", response.ProfilePicturePath);
        
        // Verify new file exists
        var newFilePath = Path.Combine(_tempPath, "profile-pictures", $"{profileId}.png");
        Assert.True(File.Exists(newFilePath));
    }

    [Fact]
    public async Task UpdateProfile_OnlyTextFields_ShouldNotAffectProfilePicture()
    {
        // Arrange
        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "text-only-update@example.com",
            PasswordHash = PasswordHasher.HashPassword("TestPass123!"),
            FullName = "Original Name",
            Age = 25,
            Gender = "Male",
            University = "Original University",
            Bio = "Original bio",
            Lifestyle = "Active",
            Interests = "Gaming",
            ProfilePicturePath = "/profile-pictures/original.jpg"
        };
        
        _dbContext.Profiles.Add(profile);
        await _dbContext.SaveChangesAsync();

        var updateRequest = new UpdateProfileRequest(
            FullName: "New Name",
            Age: 30,
            Gender: "Male",
            University: "New University",
            Bio: "New bio",
            Lifestyle: "Calm",
            Interests: "Reading, Music"
        )
        {
            UserId = profile.Id
        };

        var command = new UpdateProfileWithFileCommand(updateRequest, null);
        var handler = new UpdateProfileHandler(_dbContext, _mockEnvironment.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("New Name", response.FullName);
        Assert.Equal(30, response.Age);
        Assert.Equal("New University", response.University);
        Assert.Equal("New bio", response.Bio);
        Assert.Equal("Calm", response.Lifestyle);
        Assert.Equal("Reading, Music", response.Interests);
        // Profile picture should remain unchanged
        Assert.Equal("/profile-pictures/original.jpg", response.ProfilePicturePath);
    }

    [Fact]
    public async Task CreateProfile_WithImageWithoutExtension_ShouldDetermineExtensionFromContentType()
    {
        // Arrange
        var request = new CreateProfileRequest(
            Email: "no-ext-test@example.com",
            Password: "TestPass123!",
            FullName: "No Extension Test",
            Bio: "Test bio",
            Age: 22,
            Gender: "Female",
            University: "Test University",
            Lifestyle: "Social",
            Interests: "Music"
        );

        // File without extension but with content type
        var mockFile = CreateMockFormFile("photo", "image/jpeg", CreateFakeImageBytes());
        var command = new CreateProfileWithFileCommand(request, mockFile);

        var validator = new Mock<IValidator<CreateProfileRequest>>();
        validator.Setup(x => x.ValidateAsync(It.IsAny<CreateProfileRequest>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var handler = new CreateProfileHandler(_dbContext, _jwtService, validator.Object, _mockEnvironment.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response.ProfilePicturePath);
        // Should determine extension from content type
        Assert.True(response.ProfilePicturePath.EndsWith(".jpg") || response.ProfilePicturePath.EndsWith(".jpeg"));
    }

    [Fact]
    public async Task UpdateProfile_FirstTimeAddingPicture_ShouldSavePicture()
    {
        // Arrange - Create profile without picture
        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "first-pic@example.com",
            PasswordHash = PasswordHasher.HashPassword("TestPass123!"),
            FullName = "First Pic User",
            Age = 25,
            Gender = "Male",
            University = "Test University",
            Bio = "Test bio",
            Lifestyle = "Active",
            Interests = "Sports",
            ProfilePicturePath = null
        };
        
        _dbContext.Profiles.Add(profile);
        await _dbContext.SaveChangesAsync();

        var mockFile = CreateMockFormFile("first-photo.jpg", "image/jpeg", CreateFakeImageBytes());
        var updateRequest = new UpdateProfileRequest(
            FullName: null,
            Age: null,
            Gender: null,
            University: null,
            Bio: null,
            Lifestyle: null,
            Interests: null
        )
        {
            UserId = profile.Id
        };

        var command = new UpdateProfileWithFileCommand(updateRequest, mockFile);
        var handler = new UpdateProfileHandler(_dbContext, _mockEnvironment.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response.ProfilePicturePath);
        Assert.StartsWith("/profile-pictures/", response.ProfilePicturePath);
        
        // Verify profile in database was updated
        var updatedProfile = await _dbContext.Profiles.FindAsync(profile.Id);
        Assert.NotNull(updatedProfile?.ProfilePicturePath);
    }

    [Fact]
    public async Task CreateProfile_ProfilePicturePathContainsProfileId()
    {
        // Arrange
        var request = new CreateProfileRequest(
            Email: "id-in-path@example.com",
            Password: "TestPass123!",
            FullName: "ID Path Test",
            Bio: "Test bio",
            Age: 25,
            Gender: "Male",
            University: "Test University",
            Lifestyle: "Active",
            Interests: "Coding"
        );

        var mockFile = CreateMockFormFile("test.jpg", "image/jpeg", CreateFakeImageBytes());
        var command = new CreateProfileWithFileCommand(request, mockFile);

        var validator = new Mock<IValidator<CreateProfileRequest>>();
        validator.Setup(x => x.ValidateAsync(It.IsAny<CreateProfileRequest>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var handler = new CreateProfileHandler(_dbContext, _jwtService, validator.Object, _mockEnvironment.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(response.ProfilePicturePath);
        // The filename should contain the profile ID
        Assert.Contains(response.UserId.ToString(), response.ProfilePicturePath);
    }

    [Fact]
    public async Task UpdateProfile_EmptyFormFile_ShouldNotChangePicture()
    {
        // Arrange
        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            Email = "empty-file@example.com",
            PasswordHash = PasswordHasher.HashPassword("TestPass123!"),
            FullName = "Empty File Test",
            Age = 25,
            Gender = "Male",
            University = "Test University",
            Bio = "Test bio",
            Lifestyle = "Active",
            Interests = "Sports",
            ProfilePicturePath = "/profile-pictures/existing.jpg"
        };
        
        _dbContext.Profiles.Add(profile);
        await _dbContext.SaveChangesAsync();

        // Create empty file (0 bytes)
        var emptyFile = CreateMockFormFile("empty.jpg", "image/jpeg", Array.Empty<byte>());
        var updateRequest = new UpdateProfileRequest(
            FullName: "Updated Name",
            Age: null,
            Gender: null,
            University: null,
            Bio: null,
            Lifestyle: null,
            Interests: null
        )
        {
            UserId = profile.Id
        };

        var command = new UpdateProfileWithFileCommand(updateRequest, emptyFile);
        var handler = new UpdateProfileHandler(_dbContext, _mockEnvironment.Object);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("Updated Name", response.FullName);
        // Empty file should not change existing picture
        Assert.Equal("/profile-pictures/existing.jpg", response.ProfilePicturePath);
    }

    #endregion

    #region Helper Methods

    private static byte[] CreateFakeImageBytes()
    {
        // Create some fake image data (just random bytes for testing)
        return Encoding.UTF8.GetBytes("fake image data for testing purposes - this would be actual image bytes in production");
    }

    #endregion
}
