using MediatR;
namespace RoomMate_Finder.Features.Roommates.GetMyRequests;
public record GetMyRequestsRequest : IRequest<GetMyRequestsResponse>;
public record GetMyRequestsResponse(
    List<MyRequestDto> SentRequests,
    List<MyRequestDto> ReceivedRequests,
    List<MyRoommateDto> ActiveRoommates
);
public record MyRequestDto(
    Guid Id,
    Guid OtherUserId,
    string OtherUserName,
    string OtherUserEmail,
    string? Message,
    string Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt
);
public record MyRoommateDto(
    Guid RelationshipId,
    Guid RoommateId,
    string RoommateName,
    string RoommateEmail,
    DateTime Since
);
