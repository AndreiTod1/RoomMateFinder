using RoomMate_Finder.Features.Conversations.StartConversation;
using RoomMate_Finder.Features.Conversations.GetConversations;
using RoomMate_Finder.Features.Conversations.SendMessage;
using RoomMate_Finder.Features.Conversations.GetMessages;
using RoomMate_Finder.Features.Conversations.MarkMessagesAsRead;

namespace RoomMate_Finder.Features.Conversations;

public static class ConversationsEndpoints
{
    public static IEndpointRouteBuilder MapConversationsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapStartConversationEndpoint();
        app.MapGetConversationsEndpoint();
        app.MapSendMessageEndpoint();
        app.MapGetMessagesEndpoint();
        app.MapMarkMessagesAsReadEndpoint();
        
        return app;
    }
}
