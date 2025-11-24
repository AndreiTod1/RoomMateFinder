using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Conversations.SendMessage;

public class SendMessageHandler : IRequestHandler<SendMessageRequest, SendMessageResponse>
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SendMessageHandler(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<SendMessageResponse> Handle(SendMessageRequest request, CancellationToken cancellationToken)
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

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = currentUserId,
            Content = request.Content,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        return new SendMessageResponse(
            message.Id,
            message.ConversationId,
            message.SenderId,
            message.Content,
            message.SentAt
        );
    }
}
