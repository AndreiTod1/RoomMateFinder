using MediatR;

namespace RoomMate_Finder.Features.Reviews.GetUserReviews;

public record GetUserReviewsRequest(Guid ReviewedUserId) : IRequest<GetUserReviewsResponse>;

