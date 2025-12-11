using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Admins.GetAllUsers;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersRequest, GetAllUsersResponse>
{
    private readonly AppDbContext _dbContext;

    public GetAllUsersHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetAllUsersResponse> Handle(GetAllUsersRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Profiles.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchLower = request.Search.ToLower();
            query = query.Where(p => 
                p.FullName.ToLower().Contains(searchLower) || 
                p.Email.ToLower().Contains(searchLower));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Order by Role (Admin first), then by FullName
        var users = await query
            .OrderByDescending(p => p.Role == "Admin")
            .ThenBy(p => p.FullName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new UserDto(
                p.Id,
                p.Email,
                p.FullName,
                p.Age,
                p.Gender,
                p.University,
                p.ProfilePicturePath,
                p.CreatedAt,
                p.Role
            ))
            .ToListAsync(cancellationToken);

        return new GetAllUsersResponse(users, totalCount, request.Page, request.PageSize);
    }
}
