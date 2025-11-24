using MediatR;

namespace RoomMate_Finder.Features.Conversations.SendMessage;

public record SendMessageRequest(
    Guid ConversationId,
    string Content
) : IRequest<SendMessageResponse>;

