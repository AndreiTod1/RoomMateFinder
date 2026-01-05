namespace RoomMate_Finder.Entities;
public class RoommateRequest
{
    public Guid Id { get; set; }
    public Guid RequesterId { get; set; }
    public Guid TargetUserId { get; set; }
    public RoommateRequestStatus Status { get; set; } = RoommateRequestStatus.Pending;
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public Guid? ProcessedByAdminId { get; set; }
    public Profile Requester { get; set; } = null!;
    public Profile TargetUser { get; set; } = null!;
    public Profile? ProcessedByAdmin { get; set; }
}
public enum RoommateRequestStatus
{
    Pending = 0,           // Waiting for the other user to confirm
    MutuallyConfirmed = 1, // Both users confirmed, waiting for admin approval
    Approved = 2,          // Admin approved
    Rejected = 3           // Admin rejected or user cancelled
}
