namespace RoomMate_Finder.Features.Reviews.GetReviewStats;

public class GetReviewStatsResponse
{
    public Guid ReviewedUserId { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public Dictionary<int, int> RatingDistribution { get; set; } = new Dictionary<int, int>();
}

