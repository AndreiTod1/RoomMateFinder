namespace RoomMate_Finder.Features.Conversations.StartConversation;

public record StartConversationResponse(
    Guid ConversationId,
    Guid User1Id,
    Guid User2Id,
    DateTime CreatedAt
);

