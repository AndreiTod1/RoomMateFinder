namespace RoomMate_Finder.Features.Reviews.CreateReview;

public class CreateReviewResponse
{
    public Guid Id { get; set; }
    public Guid ReviewerId { get; set; }
    public Guid ReviewedUserId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

