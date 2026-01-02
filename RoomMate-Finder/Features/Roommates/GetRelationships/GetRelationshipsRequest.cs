using MediatR;
namespace RoomMate_Finder.Features.Roommates.GetRelationships;
public record GetRelationshipsRequest : IRequest<List<RoommateRelationshipDto>>;
public record RoommateRelationshipDto(
    Guid Id,
    Guid User1Id,
    string User1Name,
    string User1Email,
    Guid User2Id,
    string User2Name,
    string User2Email,
    string ApprovedByAdminName,
    DateTime CreatedAt,
    bool IsActive
);
