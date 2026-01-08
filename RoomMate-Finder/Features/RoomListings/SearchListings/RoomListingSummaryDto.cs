using RoomMate_Finder.Entities;

namespace RoomMate_Finder.Features.RoomListings.SearchListings;

public class RoomListingSummaryDto
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public required string OwnerFullName { get; set; }
    public required string Title { get; set; }
    public required string City { get; set; }
    public required string Area { get; set; }
    public decimal Price { get; set; }
    public DateTime AvailableFrom { get; set; }
    public List<string> Amenities { get; set; } = new();
    public bool IsActive { get; set; }
    public string? ThumbnailPath { get; set; }
    public ListingApprovalStatus ApprovalStatus { get; set; }
    public string? RejectionReason { get; set; }
}

