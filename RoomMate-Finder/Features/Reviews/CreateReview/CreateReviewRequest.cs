using MediatR;

namespace RoomMate_Finder.Features.Reviews.CreateReview;

public class CreateReviewRequest : IRequest<CreateReviewResponse>
{
    public Guid ReviewerId { get; set; }
    public Guid ReviewedUserId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

