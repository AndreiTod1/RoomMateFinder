using MediatR;

namespace RoomMate_Finder.Features.Conversations.GetUnreadConversations;

public record GetUnreadConversationsRequest : IRequest<GetUnreadConversationsResponse>;

