using MediatR;

namespace RoomMate_Finder.Features.Admins.GetAllUsers;

public record GetAllUsersRequest(int Page = 1, int PageSize = 12, string? Search = null) : IRequest<GetAllUsersResponse>;
