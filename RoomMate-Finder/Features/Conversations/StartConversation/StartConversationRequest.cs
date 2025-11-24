using MediatR;

namespace RoomMate_Finder.Features.Conversations.StartConversation;

public record StartConversationRequest(
    Guid OtherUserId
) : IRequest<StartConversationResponse>;

