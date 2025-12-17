using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Conversations.GetUnreadConversations;

public class GetUnreadConversationsHandler : IRequestHandler<GetUnreadConversationsRequest, GetUnreadConversationsResponse>
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetUnreadConversationsHandler(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<GetUnreadConversationsResponse> Handle(GetUnreadConversationsRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = (Guid)_httpContextAccessor.HttpContext!.Items["CurrentUserId"]!;

        // Get all unread messages for this user (messages sent by others that are not read)
        var unreadMessages = await _context.Messages
            .Include(m => m.Conversation)
            .Where(m => !m.IsRead 
                && m.SenderId != currentUserId
                && (m.Conversation.User1Id == currentUserId || m.Conversation.User2Id == currentUserId))
            .GroupBy(m => m.ConversationId)
            .Select(g => new UnreadConversationDto(
                g.Key,
                g.Count()
            ))
            .ToListAsync(cancellationToken);

        return new GetUnreadConversationsResponse(
            unreadMessages,
            unreadMessages.Sum(x => x.UnreadCount)
        );
    }
}

