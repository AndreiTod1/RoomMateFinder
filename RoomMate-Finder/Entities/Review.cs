using System;

namespace RoomMate_Finder.Entities;

public class Review
{
    public Guid Id { get; set; }

    // The user who wrote the review
    public Guid ReviewerId { get; set; }
    public Profile Reviewer { get; set; } = null!;

    // The user who is being reviewed
    public Guid ReviewedUserId { get; set; }
    public Profile ReviewedUser { get; set; } = null!;

    // Rating 1-5
    public int Rating { get; set; }

    // Optional comment
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
