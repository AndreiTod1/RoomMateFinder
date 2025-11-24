using MediatR;

namespace RoomMate_Finder.Features.Conversations.GetConversations;

public record GetConversationsRequest : IRequest<GetConversationsResponse>;

