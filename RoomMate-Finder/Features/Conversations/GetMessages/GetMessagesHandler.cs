using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Conversations.GetMessages;

public class GetMessagesHandler : IRequestHandler<GetMessagesRequest, GetMessagesResponse>
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetMessagesHandler(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<GetMessagesResponse> Handle(GetMessagesRequest request, CancellationToken cancellationToken)
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

        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == request.ConversationId)
            .OrderBy(m => m.SentAt)
            .ToListAsync(cancellationToken);

        var messageDtos = messages.Select(m => new MessageDto(
            m.Id,
            m.SenderId,
            m.Sender.FullName,
            m.Sender.Role,
            m.Content,
            m.SentAt,
            m.IsRead
        )).ToList();

        return new GetMessagesResponse(messageDtos);
    }
}
