using MediatR;

namespace RoomMate_Finder.Features.Conversations.GetMessages;

public record GetMessagesRequest(
    Guid ConversationId
) : IRequest<GetMessagesResponse>;