namespace RoomMate_Finder.Features.Conversations.GetUnreadConversations;

public record GetUnreadConversationsResponse(
    List<UnreadConversationDto> UnreadConversations,
    int TotalUnreadMessages
);

public record UnreadConversationDto(
    Guid ConversationId,
    int UnreadCount
);

