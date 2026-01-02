using MediatR;

namespace RoomMate_Finder.Features.Roommates.GetUserRoommate;

public record GetUserRoommateRequest(Guid UserId) : IRequest<UserRoommateResponse?>;

public record UserRoommateResponse(
    Guid RelationshipId,
    Guid RoommateId,
    string RoommateName,
    string RoommateEmail,
    string? ProfilePicturePath,
    int Age,
    string? University,
    DateTime Since
);

