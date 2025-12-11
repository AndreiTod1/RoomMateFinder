using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Admins.UpdateUserRole;

public class UpdateUserRoleHandler : IRequestHandler<UpdateUserRoleRequest>
{
    private readonly AppDbContext _dbContext;

    public UpdateUserRoleHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        var profile = await _dbContext.Profiles.FindAsync(new object[] { request.Id }, cancellationToken);
        if (profile == null)
        {
            throw new KeyNotFoundException($"Profile with ID {request.Id} not found.");
        }

        if (request.Role != "Admin" && request.Role != "User")
        {
            throw new ArgumentException("Invalid role. Must be 'Admin' or 'User'.");
        }

        profile.Role = request.Role;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
