using MediatR;
using Microsoft.EntityFrameworkCore;
using RoomMate_Finder.Entities;
using RoomMate_Finder.Infrastructure.Persistence;

namespace RoomMate_Finder.Features.Conversations.StartConversation;

public class StartConversationHandler : IRequestHandler<StartConversationRequest, StartConversationResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public StartConversationHandler(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<StartConversationResponse> Handle(StartConversationRequest request, CancellationToken cancellationToken)
    {
        // Get CurrentUserId from HttpContext (set by endpoint from JWT token)
        var currentUserId = (Guid)_httpContextAccessor.HttpContext!.Items["CurrentUserId"]!;

        // Validate that the other user exists
        var otherUser = await _dbContext.Profiles
            .FirstOrDefaultAsync(p => p.Id == request.OtherUserId, cancellationToken);
            
        if (otherUser == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        
        // Cannot start conversation with yourself
        if (currentUserId == request.OtherUserId)
        {
            throw new InvalidOperationException("Cannot start a conversation with yourself");
        }
        
        // Check if conversation already exists (in either direction)
        var existingConversation = await _dbContext.Conversations
            .FirstOrDefaultAsync(c => 
                (c.User1Id == currentUserId && c.User2Id == request.OtherUserId) ||
                (c.User1Id == request.OtherUserId && c.User2Id == currentUserId),
                cancellationToken);
                
        if (existingConversation != null)
        {
            return new StartConversationResponse(
                existingConversation.Id,
                existingConversation.User1Id,
                existingConversation.User2Id,
                existingConversation.CreatedAt
            );
        }
        
        // Create new conversation (always store with lower GUID first for consistency)
        var user1Id = currentUserId < request.OtherUserId ? currentUserId : request.OtherUserId;
        var user2Id = currentUserId < request.OtherUserId ? request.OtherUserId : currentUserId;
        
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            User1Id = user1Id,
            User2Id = user2Id,
            CreatedAt = DateTime.UtcNow
        };
        
        _dbContext.Conversations.Add(conversation);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return new StartConversationResponse(
            conversation.Id,
            conversation.User1Id,
            conversation.User2Id,
            conversation.CreatedAt
        );
    }
}
