﻿namespace RoomMate_Finder_Frontend.Services;

public interface IConversationService
{
    Task<List<ConversationDto>> GetConversationsAsync();
    Task<List<MessageDto>> GetMessagesAsync(Guid conversationId);
    Task<ConversationDto?> StartConversationAsync(Guid otherUserId);
    Task<MessageDto?> SendMessageAsync(Guid conversationId, string content);
    Task MarkMessagesAsReadAsync(Guid conversationId);
    Task<UnreadConversationsResponse?> GetUnreadConversationsAsync();
}

public record ConversationDto(
    Guid Id,
    Guid OtherUserId,
    string OtherUserName,
    string? OtherUserProfilePicture,
    string? OtherUserRole,
    DateTime CreatedAt
);

public record MessageDto(
    Guid Id,
    Guid SenderId,
    string SenderName,
    string? SenderRole,
    string Content,
    DateTime SentAt,
    bool IsRead
);

public record UnreadConversationsResponse(
    List<UnreadConversationDto> UnreadConversations,
    int TotalUnreadMessages
);

public record UnreadConversationDto(
    Guid ConversationId,
    int UnreadCount
);

