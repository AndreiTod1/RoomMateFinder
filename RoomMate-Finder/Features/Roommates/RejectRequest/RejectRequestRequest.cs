using MediatR;

namespace RoomMate_Finder.Features.Roommates.RejectRequest;
public record RejectRequestResponse(string Message);

public record RejectRequestRequest(Guid RequestId) : IRequest<RejectRequestResponse>;




