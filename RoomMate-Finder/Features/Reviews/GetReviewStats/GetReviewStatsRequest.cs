using MediatR;

namespace RoomMate_Finder.Features.Reviews.GetReviewStats;

public record GetReviewStatsRequest(Guid ReviewedUserId) : IRequest<GetReviewStatsResponse>;

