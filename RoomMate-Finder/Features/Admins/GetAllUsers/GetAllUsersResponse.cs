namespace RoomMate_Finder.Features.Admins.GetAllUsers;

public record GetAllUsersResponse(
    List<UserDto> Users,
    int TotalCount,
    int Page,
    int PageSize
);

public record UserDto(
    Guid Id,
    string Email,
    string FullName,
    int Age,
    string Gender,
    string University,
    string? ProfilePicturePath,
    DateTime CreatedAt,
    string Role
);
