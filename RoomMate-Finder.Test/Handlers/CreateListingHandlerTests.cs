using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Features.RoomListings.CreateListing;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Test.Handlers;

public class CreateListingHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly CreateListingHandler _handler;
    private readonly string _tempPath;

    public CreateListingHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);

        _tempPath = Path.Combine(Path.GetTempPath(), "CreateListingTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempPath);

        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockEnvironment.Setup(e => e.WebRootPath).Returns(_tempPath);
        // Fallback if code checks ContentRootPath
        _mockEnvironment.Setup(e => e.ContentRootPath).Returns(_tempPath);

        _handler = new CreateListingHandler(_context, _mockEnvironment.Object);
    }

    [Fact]
    public async Task Handle_GivenValidRequest_WhenOwnerExists_ShouldCreateListing_AndReturnResponse()
    {
        // Arrange
        var owner = new Profile 
        { 
            Id = Guid.NewGuid(), 
            FullName = "Test Owner",
            Email = "test@example.com",
            PasswordHash = "hash",
            Gender = "M",
            Age = 20,
            University = "Uni",
            Bio = "Bio",
            Interests = "None",
            Lifestyle = "Active",
            Role = "User"
        };
        _context.Profiles.Add(owner);
        await _context.SaveChangesAsync();

        var request = new CreateListingRequest
        {
            OwnerId = owner.Id,
            Title = "Nice Room",
            Description = "Very nice",
            City = "Cluj",
            Area = "Center",
            Price = 200,
            AvailableFrom = DateTime.UtcNow.AddDays(1),
            Amenities = new List<string> { "Wifi", " AC " }
        };

        var command = new CreateListingWithImagesCommand(request, new List<IFormFile>(), false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Nice Room");
        result.Amenities.Should().Contain("Wifi");
        result.Amenities.Should().Contain("AC");
        result.ApprovalStatus.Should().Be(ListingApprovalStatus.Pending);

        var listingInDb = await _context.RoomListings.FindAsync(result.Id);
        listingInDb.Should().NotBeNull();
        listingInDb!.OwnerId.Should().Be(owner.Id);
    }

    [Fact]
    public async Task Handle_GivenImages_ShouldSaveImages_AndSetPaths()
    {
        // Arrange
        var owner = new Profile 
        { 
            Id = Guid.NewGuid(), 
            FullName = "Owner",
            Email = "o@e.com",
            PasswordHash = "h",
            Gender = "F",
            Age = 22,
            University = "U",
            Bio = "B",
            Interests = "I",
            Lifestyle = "L",
            Role = "User"
        };
        _context.Profiles.Add(owner);
        await _context.SaveChangesAsync();

        var request = new CreateListingRequest
        {
            OwnerId = owner.Id,
            Title = "Room with Images",
            Description = "Desc",
            City = "City",
            Area = "Area",
            Price = 100,
            AvailableFrom = DateTime.UtcNow,
            Amenities = new List<string>()
        };

        var imageFile = CreateMockFile("test.jpg", "image/jpeg", 100);
        var images = new List<IFormFile> { imageFile };

        var command = new CreateListingWithImagesCommand(request, images, false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ImagePaths.Should().HaveCount(1);
        result.ImagePaths[0].Should().StartWith("/room-images/");
        result.ImagePaths[0].Should().EndWith(".jpg");

        // Verify file exists (mock environment points to temp path)
        var fileName = result.ImagePaths[0].Replace("/room-images/", "");
        var fullPath = Path.Combine(_tempPath, "room-images", fileName);
        File.Exists(fullPath).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_GivenNonExistentOwner_ShouldThrowException()
    {
        // Arrange
        var request = new CreateListingRequest { OwnerId = Guid.NewGuid() }; // No such owner
        var command = new CreateListingWithImagesCommand(request, new List<IFormFile>(), false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Owner profile not found.");
    }

    [Fact]
    public async Task Handle_GivenTooManyImages_ShouldThrowException()
    {
        // Arrange
        var owner = new Profile { Id = Guid.NewGuid(), FullName="O", Email="E", PasswordHash="P", Gender="M", Age=20, University="U", Bio="B", Interests="I", Lifestyle="L", Role="User" };
        _context.Profiles.Add(owner);
        await _context.SaveChangesAsync();

        var request = new CreateListingRequest { OwnerId = owner.Id };
        var images = Enumerable.Range(0, 9) // 9 images, max is 8
            .Select(i => CreateMockFile($"img{i}.jpg", "image/jpeg", 100))
            .ToList();

        var command = new CreateListingWithImagesCommand(request, images, false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Maximum 8 images allowed per listing.");
    }

    [Fact]
    public async Task Handle_GivenInvalidImageType_ShouldThrowException()
    {
        // Arrange
        var owner = new Profile { Id = Guid.NewGuid(), FullName="O", Email="E", PasswordHash="P", Gender="M", Age=20, University="U", Bio="B", Interests="I", Lifestyle="L", Role="User" };
        _context.Profiles.Add(owner);
        await _context.SaveChangesAsync();

        var request = new CreateListingRequest { OwnerId = owner.Id };
        var image = CreateMockFile("test.txt", "text/plain", 100); // Invalid type
        var command = new CreateListingWithImagesCommand(request, new List<IFormFile> { image }, false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid image type*");
    }

    [Fact]
    public async Task Handle_GivenTooLargeImage_ShouldThrowException()
    {
        // Arrange
        var owner = new Profile { Id = Guid.NewGuid(), FullName="O", Email="E", PasswordHash="P", Gender="M", Age=20, University="U", Bio="B", Interests="I", Lifestyle="L", Role="User" };
        _context.Profiles.Add(owner);
        await _context.SaveChangesAsync();

        long size = (5 * 1024 * 1024) + 1; // 5MB + 1 byte
        var request = new CreateListingRequest { OwnerId = owner.Id };
        var image = CreateMockFile("large.jpg", "image/jpeg", size); 
        var command = new CreateListingWithImagesCommand(request, new List<IFormFile> { image }, false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Image file size must be less than 5MB.");
    }

    [Fact]
    public async Task Handle_GivenAdminRequest_ShouldAutoApprove()
    {
        // Arrange
        var admin = new Profile { Id = Guid.NewGuid(), FullName="Admin", Email="a@admin.com", PasswordHash="P", Gender="M", Age=30, University="U", Bio="B", Interests="I", Lifestyle="L", Role="Admin" };
        _context.Profiles.Add(admin);
        await _context.SaveChangesAsync();

        var request = new CreateListingRequest
        {
            OwnerId = admin.Id,
            Title = "Admin Room",
            Description = "D",
            City = "C",
            Area = "A",
            Price = 100,
            AvailableFrom = DateTime.UtcNow,
            Amenities = new List<string>()
        };

        var command = new CreateListingWithImagesCommand(request, new List<IFormFile>(), IsAdmin: true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ApprovalStatus.Should().Be(ListingApprovalStatus.Approved);
        
        var listing = await _context.RoomListings.FindAsync(result.Id);
        listing!.ApprovedByAdminId.Should().Be(admin.Id);
        listing.ApprovedAt.Should().NotBeNull();
    }

    private static IFormFile CreateMockFile(string fileName, string contentType, long length)
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.Length).Returns(length);
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Callback<Stream, CancellationToken>((stream, token) => 
            {
                // simulate writing something
                var bytes = new byte[10]; 
                stream.Write(bytes, 0, bytes.Length);
            })
            .Returns(Task.CompletedTask);
        return fileMock.Object;
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        if (Directory.Exists(_tempPath))
        {
            try { Directory.Delete(_tempPath, true); } catch { }
        }
        GC.SuppressFinalize(this);
    }
}
