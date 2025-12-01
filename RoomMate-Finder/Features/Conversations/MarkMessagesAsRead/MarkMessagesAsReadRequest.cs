using MediatR;

namespace RoomMate_Finder.Features.Conversations.MarkMessagesAsRead;

public record MarkMessagesAsReadRequest(
    Guid ConversationId
) : IRequest<MarkMessagesAsReadResponse>;

