using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.RoomListings.CreateListing;

public class CreateListingHandler : IRequestHandler<CreateListingWithImagesCommand, CreateListingResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public CreateListingHandler(AppDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task<CreateListingResponse> Handle(CreateListingWithImagesCommand command, CancellationToken cancellationToken)
    {
        var request = command.Listing;
        
        var owner = await _dbContext.Profiles
            .FirstOrDefaultAsync(p => p.Id == request.OwnerId, cancellationToken);

        if (owner == null)
        {
            throw new InvalidOperationException("Owner profile not found.");
        }

        // Validate image count
        if (command.Images != null && command.Images.Count > 8)
        {
            throw new InvalidOperationException("Maximum 8 images allowed per listing.");
        }

        var listingId = Guid.NewGuid();
        var imagePaths = new List<string>();
        
        Console.WriteLine($"[CreateListingHandler] Received command with {command.Images?.Count ?? 0} images");

        // Save images - copying logic from profile picture
        if (command.Images != null && command.Images.Count > 0)
        {
            // Get wwwroot path like profile picture does
            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "wwwroot", "room-images");
            
            // If WebRootPath is set, use it directly
            if (!string.IsNullOrEmpty(_environment.WebRootPath))
            {
                uploadsFolder = Path.Combine(_environment.WebRootPath, "room-images");
            }
            
            Console.WriteLine($"[CreateListingHandler] Upload folder: {uploadsFolder}");
            
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
                Console.WriteLine($"[CreateListingHandler] Created directory: {uploadsFolder}");
            }

            var displayOrder = 0;
            foreach (var image in command.Images)
            {
                if (image.Length > 0)
                {
                    Console.WriteLine($"[CreateListingHandler] Processing image: {image.FileName}, Size: {image.Length}, ContentType: {image.ContentType}");
                    
                    // Validate file type
                    var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
                    if (!allowedTypes.Contains(image.ContentType.ToLower()))
                    {
                        throw new InvalidOperationException($"Invalid image type: {image.ContentType}. Allowed types: jpg, png, webp");
                    }

                    // Validate file size (5MB max)
                    if (image.Length > 5 * 1024 * 1024)
                    {
                        throw new InvalidOperationException("Image file size must be less than 5MB.");
                    }

                    var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                    if (string.IsNullOrEmpty(extension))
                    {
                        extension = image.ContentType switch
                        {
                            "image/jpeg" => ".jpg",
                            "image/jpg" => ".jpg",
                            "image/png" => ".png",
                            "image/webp" => ".webp",
                            _ => ".jpg"
                        };
                    }

                    var fileName = $"{listingId}_{displayOrder}_{Guid.NewGuid():N}{extension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Save file like profile picture does
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream, cancellationToken);
                    }
                    
                    Console.WriteLine($"[CreateListingHandler] Saved image to: {filePath}");

                    var relativePath = $"/room-images/{fileName}";
                    imagePaths.Add(relativePath);
                    displayOrder++;
                }
            }
        }

        var listing = new RoomListing
        {
            Id = listingId,
            OwnerId = request.OwnerId,
            Title = request.Title,
            Description = request.Description,
            City = request.City,
            Area = request.Area,
            Price = request.Price,
            AvailableFrom = DateTime.SpecifyKind(request.AvailableFrom, DateTimeKind.Utc),
            Amenities = string.Join(",", request.Amenities
                .Select(a => a.Trim())
                .Where(a => !string.IsNullOrWhiteSpace(a))),
            ImagePaths = string.Join(",", imagePaths),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            // Admin-created listings are auto-approved, user-created listings need approval
            ApprovalStatus = command.IsAdmin ? ListingApprovalStatus.Approved : ListingApprovalStatus.Pending,
            ApprovedByAdminId = command.IsAdmin ? request.OwnerId : null,
            ApprovedAt = command.IsAdmin ? DateTime.UtcNow : null
        };

        Console.WriteLine($"[CreateListingHandler] Saving listing with ImagePaths: {listing.ImagePaths}");

        _dbContext.RoomListings.Add(listing);
        await _dbContext.SaveChangesAsync(cancellationToken);

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
            Amenities = listing.Amenities
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList(),
            CreatedAt = listing.CreatedAt,
            IsActive = listing.IsActive,
            ImagePaths = imagePaths,
            ApprovalStatus = listing.ApprovalStatus
        };
    }
}
