using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Roommates.GetRelationships;

public class GetRelationshipsHandler : IRequestHandler<GetRelationshipsRequest, List<RoommateRelationshipDto>>
{
    private readonly AppDbContext _context;

    public GetRelationshipsHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoommateRelationshipDto>> Handle(GetRelationshipsRequest request, CancellationToken cancellationToken)
    {
        var relationships = await _context.RoommateRelationships
            .Include(r => r.User1)
            .Include(r => r.User2)
            .Include(r => r.ApprovedByAdmin)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RoommateRelationshipDto(
                r.Id,
                r.User1Id,
                r.User1.FullName,
                r.User1.Email,
                r.User2Id,
                r.User2.FullName,
                r.User2.Email,
                r.ApprovedByAdmin.FullName,
                r.CreatedAt,
                r.IsActive
            ))
            .ToListAsync(cancellationToken);

        return relationships;
    }
}

