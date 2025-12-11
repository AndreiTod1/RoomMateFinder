using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Admins.DeleteProfile;

public class DeleteProfileHandler : IRequestHandler<DeleteProfileRequest>
{
    private readonly AppDbContext _dbContext;

    public DeleteProfileHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await _dbContext.Profiles.FindAsync(new object[] { request.Id }, cancellationToken);
        if (profile == null)
        {
            throw new KeyNotFoundException($"Profile with ID {request.Id} not found.");
        }

        _dbContext.Profiles.Remove(profile);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
