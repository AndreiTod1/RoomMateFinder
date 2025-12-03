namespace RoomMate_Finder.Features.Reviews.GetUserReviews;

public class GetUserReviewsResponse
{
    public IEnumerable<ReviewDto> Reviews { get; set; } = Enumerable.Empty<ReviewDto>();

    public class ReviewDto
    {
        public Guid Id { get; set; }
        public Guid ReviewerId { get; set; }
        public string ReviewerFullName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

