using MediatR;
namespace RoomMate_Finder.Features.Roommates.ApproveRequest;
public record ApproveRequestRequest(Guid RequestId) : IRequest<ApproveRequestResponse>;
public record ApproveRequestResponse(Guid RelationshipId, string Message);
