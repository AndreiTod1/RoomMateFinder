using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Roommates.DeleteRelationship;

public class DeleteRelationshipHandler : IRequestHandler<DeleteRelationshipRequest, DeleteRelationshipResponse>
{
    private readonly AppDbContext _context;

    public DeleteRelationshipHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteRelationshipResponse> Handle(DeleteRelationshipRequest request, CancellationToken cancellationToken)
    {
        var relationship = await _context.RoommateRelationships
            .Include(r => r.User1)
            .Include(r => r.User2)
            .FirstOrDefaultAsync(r => r.Id == request.RelationshipId, cancellationToken);

        if (relationship == null)
        {
            throw new InvalidOperationException("Relationship not found");
        }

        var user1Name = relationship.User1.FullName;
        var user2Name = relationship.User2.FullName;

        // Soft delete - just mark as inactive
        relationship.IsActive = false;
        
        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteRelationshipResponse($"Roommate relationship between {user1Name} and {user2Name} has been deactivated");
    }
}

