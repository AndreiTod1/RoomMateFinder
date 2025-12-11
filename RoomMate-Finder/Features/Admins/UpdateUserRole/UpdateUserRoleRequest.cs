using MediatR;

namespace RoomMate_Finder.Features.Admins.UpdateUserRole;

public record UpdateUserRoleRequest(Guid Id, string Role) : IRequest;
