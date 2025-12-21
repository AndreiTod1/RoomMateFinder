using MediatR;
namespace RoomMate_Finder.Features.Roommates.DeleteRelationship;
public record DeleteRelationshipRequest(Guid RelationshipId) : IRequest<DeleteRelationshipResponse>;
public record DeleteRelationshipResponse(string Message);
