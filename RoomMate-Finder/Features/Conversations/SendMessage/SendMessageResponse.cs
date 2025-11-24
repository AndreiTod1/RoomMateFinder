namespace RoomMate_Finder.Features.Conversations.SendMessage;

public record SendMessageResponse(
    Guid MessageId,
    Guid ConversationId,
    Guid SenderId,
    string Content,
    DateTime SentAt
);

