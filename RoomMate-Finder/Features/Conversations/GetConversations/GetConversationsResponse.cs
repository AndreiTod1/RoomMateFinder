namespace RoomMate_Finder.Features.Conversations.GetConversations;

public record GetConversationsResponse(
    List<ConversationDto> Conversations
);

public record ConversationDto(
    Guid Id,
    Guid OtherUserId,
    string OtherUserName,
    DateTime CreatedAt
);

