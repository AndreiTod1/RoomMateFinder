namespace RoomMate_Finder.Features.Conversations.GetMessages;

public record GetMessagesResponse(
    List<MessageDto> Messages
);

public record MessageDto(
    Guid Id,
    Guid SenderId,
    string SenderName,
    string Content,
    DateTime SentAt,
    bool IsRead
);

