using RoomMate_Finder.Features.Conversations.StartConversation;

namespace RoomMate_Finder.Features.Conversations;

public static class ConversationsEndpoints
{
    public static IEndpointRouteBuilder MapConversationsEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapStartConversationEndpoint();
        
        return app;
    }
}
