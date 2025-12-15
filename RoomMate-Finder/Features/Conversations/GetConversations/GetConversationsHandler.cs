using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Conversations.GetConversations;

public class GetConversationsHandler : IRequestHandler<GetConversationsRequest, GetConversationsResponse>
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetConversationsHandler(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<GetConversationsResponse> Handle(GetConversationsRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = (Guid)_httpContextAccessor.HttpContext!.Items["CurrentUserId"]!;

        var conversations = await _context.Conversations
            .Include(c => c.User1)
            .Include(c => c.User2)
            .Where(c => c.User1Id == currentUserId || c.User2Id == currentUserId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var conversationDtos = conversations.Select(c =>
        {
            var isUser1 = c.User1Id == currentUserId;
            var otherUser = isUser1 ? c.User2 : c.User1;

            return new ConversationDto(
                c.Id,
                otherUser.Id,
                otherUser.FullName,
                otherUser.ProfilePicturePath,
                otherUser.Role,
                c.CreatedAt
            );
        }).ToList();

        return new GetConversationsResponse(conversationDtos);
    }
}
