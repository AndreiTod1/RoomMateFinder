using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Roommates.GetUserRoommate;

public class GetUserRoommateHandler : IRequestHandler<GetUserRoommateRequest, UserRoommateResponse?>
{
    private readonly AppDbContext _context;

    public GetUserRoommateHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserRoommateResponse?> Handle(GetUserRoommateRequest request, CancellationToken cancellationToken)
    {
        var relationship = await _context.RoommateRelationships
            .Include(r => r.User1)
            .Include(r => r.User2)
            .Where(r => r.IsActive && (r.User1Id == request.UserId || r.User2Id == request.UserId))
            .FirstOrDefaultAsync(cancellationToken);

        if (relationship == null)
            return null;

        var isUser1 = relationship.User1Id == request.UserId;
        var roommate = isUser1 ? relationship.User2 : relationship.User1;

        return new UserRoommateResponse(
            relationship.Id,
            roommate.Id,
            roommate.FullName,
            roommate.Email,
            roommate.ProfilePicturePath,
            roommate.Age,
            roommate.University,
            relationship.CreatedAt
        );
    }
}

