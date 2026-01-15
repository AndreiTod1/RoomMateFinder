using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.RoomListings.CreateListing;

public class CreateListingHandler : IRequestHandler<CreateListingWithImagesCommand, CreateListingResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/jpg", "image/png", "image/webp"];
    private const int MaxImageCount = 8;
    private const int MaxImageSizeBytes = 5 * 1024 * 1024; // 5MB

    public CreateListingHandler(AppDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task<CreateListingResponse> Handle(CreateListingWithImagesCommand command, CancellationToken cancellationToken)
    {
        var request = command.Listing;
        
        await ValidateOwnerExistsAsync(request.OwnerId, cancellationToken);
        ValidateImageCount(command.Images);

        var listingId = Guid.NewGuid();
        var imagePaths = await SaveImagesAsync(listingId, command.Images, cancellationToken);

        var listing = CreateListing(listingId, request, imagePaths, command.IsAdmin);

        _dbContext.RoomListings.Add(listing);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreateResponse(listing, imagePaths);
    }

    private async Task ValidateOwnerExistsAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        var ownerExists = await _dbContext.Profiles.AnyAsync(p => p.Id == ownerId, cancellationToken);
        if (!ownerExists)
        {
            throw new InvalidOperationException("Owner profile not found.");
        }
    }

    private static void ValidateImageCount(IReadOnlyList<IFormFile>? images)
    {
        if (images != null && images.Count > MaxImageCount)
        {
            throw new InvalidOperationException($"Maximum {MaxImageCount} images allowed per listing.");
        }
    }

    private async Task<List<string>> SaveImagesAsync(Guid listingId, IReadOnlyList<IFormFile>? images, CancellationToken cancellationToken)
    {
        var imagePaths = new List<string>();
        
        if (images == null || images.Count == 0)
            return imagePaths;

        var uploadsFolder = GetUploadsFolder();
        EnsureDirectoryExists(uploadsFolder);

        var displayOrder = 0;
        foreach (var image in images)
        {
            if (image.Length > 0)
            {
                var relativePath = await SaveSingleImageAsync(listingId, image, uploadsFolder, displayOrder, cancellationToken);
                imagePaths.Add(relativePath);
                displayOrder++;
            }
        }

        return imagePaths;
    }

    private static async Task<string> SaveSingleImageAsync(Guid listingId, IFormFile image, string uploadsFolder, int displayOrder, CancellationToken cancellationToken)
    {
        ValidateImageType(image);
        ValidateImageSize(image);

        var extension = GetFileExtension(image);
        var fileName = $"{listingId}_{displayOrder}_{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream, cancellationToken);
        }

        return $"/room-images/{fileName}";
    }

    private static void ValidateImageType(IFormFile image)
    {
        if (!AllowedImageTypes.Contains(image.ContentType.ToLower()))
        {
            throw new InvalidOperationException($"Invalid image type: {image.ContentType}. Allowed types: jpg, png, webp");
        }
    }

    private static void ValidateImageSize(IFormFile image)
    {
        if (image.Length > MaxImageSizeBytes)
        {
            throw new InvalidOperationException("Image file size must be less than 5MB.");
        }
    }

    private static string GetFileExtension(IFormFile image)
    {
        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!string.IsNullOrEmpty(extension))
            return extension;

        return image.ContentType switch
        {
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };
    }

    private string GetUploadsFolder()
    {
        if (!string.IsNullOrEmpty(_environment.WebRootPath))
        {
            return Path.Combine(_environment.WebRootPath, "room-images");
        }
        return Path.Combine(_environment.ContentRootPath, "wwwroot", "room-images");
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private static RoomListing CreateListing(Guid listingId, CreateListingRequest request, List<string> imagePaths, bool isAdmin)
    {
        return new RoomListing
        {
            Id = listingId,
            OwnerId = request.OwnerId,
            Title = request.Title,
            Description = request.Description,
            City = request.City,
            Area = request.Area,
            Price = request.Price,
            AvailableFrom = DateTime.SpecifyKind(request.AvailableFrom, DateTimeKind.Utc),
            Amenities = string.Join(",", request.Amenities.Select(a => a.Trim()).Where(a => !string.IsNullOrWhiteSpace(a))),
            ImagePaths = string.Join(",", imagePaths),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            ApprovalStatus = isAdmin ? ListingApprovalStatus.Approved : ListingApprovalStatus.Pending,
            ApprovedByAdminId = isAdmin ? request.OwnerId : null,
            ApprovedAt = isAdmin ? DateTime.UtcNow : null
        };
    }

    private static CreateListingResponse CreateResponse(RoomListing listing, List<string> imagePaths)
    {
        return new CreateListingResponse
        {
            Id = listing.Id,
            OwnerId = listing.OwnerId,
            Title = listing.Title,
            Description = listing.Description,
            City = listing.City,
            Area = listing.Area,
            Price = listing.Price,
            AvailableFrom = listing.AvailableFrom,
            Amenities = listing.Amenities.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            CreatedAt = listing.CreatedAt,
            IsActive = listing.IsActive,
            ImagePaths = imagePaths,
            ApprovalStatus = listing.ApprovalStatus
        };
    }
}
