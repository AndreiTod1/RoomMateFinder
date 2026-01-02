using MediatR;
namespace RoomMate_Finder.Features.Roommates.GetPendingRequests;
public record GetPendingRequestsRequest : IRequest<List<PendingRequestDto>>;
public record PendingRequestDto(
    Guid Id,
    Guid RequesterId,
    string RequesterName,
    string RequesterEmail,
    Guid TargetUserId,
    string TargetUserName,
    string TargetUserEmail,
    string? Message,
    DateTime CreatedAt
);
