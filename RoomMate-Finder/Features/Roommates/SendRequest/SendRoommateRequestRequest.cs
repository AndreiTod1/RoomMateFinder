using MediatR;
namespace RoomMate_Finder.Features.Roommates.SendRequest;
public record SendRoommateRequestRequest(Guid TargetUserId, string? Message) : IRequest<SendRoommateRequestResponse>;
public record SendRoommateRequestResponse(Guid Id, string Message);
