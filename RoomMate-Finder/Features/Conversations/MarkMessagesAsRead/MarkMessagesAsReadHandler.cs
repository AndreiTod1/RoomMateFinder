using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Conversations.MarkMessagesAsRead;

public class MarkMessagesAsReadHandler : IRequestHandler<MarkMessagesAsReadRequest, MarkMessagesAsReadResponse>
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MarkMessagesAsReadHandler(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<MarkMessagesAsReadResponse> Handle(MarkMessagesAsReadRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = (Guid)_httpContextAccessor.HttpContext!.Items["CurrentUserId"]!;

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

        if (conversation == null)
        {
            throw new KeyNotFoundException($"Conversation with ID {request.ConversationId} not found");
        }

        if (conversation.User1Id != currentUserId && conversation.User2Id != currentUserId)
        {
            throw new UnauthorizedAccessException("You are not a participant in this conversation");
        }

        // Only mark messages sent by the other user
        var unreadMessages = await _context.Messages
            .Where(m => m.ConversationId == request.ConversationId 
                     && m.SenderId != currentUserId 
                     && !m.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new MarkMessagesAsReadResponse(unreadMessages.Count);
    }
}
