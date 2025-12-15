using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RoomMate_Finder.Infrastructure.Persistence;
using RoomMate_Finder.Entities;
using Microsoft.EntityFrameworkCore;

namespace RoomMate_Finder.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly AppDbContext _dbContext;
    private static readonly Dictionary<Guid, HashSet<string>> _userConnections = new();
    private static readonly object _lock = new();

    public ChatHub(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId != Guid.Empty)
        {
            lock (_lock)
            {
                if (!_userConnections.ContainsKey(userId))
                    _userConnections[userId] = new HashSet<string>();
                _userConnections[userId].Add(Context.ConnectionId);
            }
            
            Console.WriteLine($"[ChatHub] User {userId} connected with connection {Context.ConnectionId}");
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId != Guid.Empty)
        {
            lock (_lock)
            {
                if (_userConnections.ContainsKey(userId))
                {
                    _userConnections[userId].Remove(Context.ConnectionId);
                    if (_userConnections[userId].Count == 0)
                        _userConnections.Remove(userId);
                }
            }
            
            Console.WriteLine($"[ChatHub] User {userId} disconnected");
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinConversation(Guid conversationId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return;

        // Verify user is part of the conversation
        var conversation = await _dbContext.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId 
                && (c.User1Id == userId || c.User2Id == userId));

        if (conversation != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            Console.WriteLine($"[ChatHub] User {userId} joined conversation {conversationId}");
        }
    }

    public async Task LeaveConversation(Guid conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        Console.WriteLine($"[ChatHub] User {GetUserId()} left conversation {conversationId}");
    }

    public async Task SendMessage(Guid conversationId, string content)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(content)) return;

        // Verify user is part of the conversation
        var conversation = await _dbContext.Conversations
            .Include(c => c.User1)
            .Include(c => c.User2)
            .FirstOrDefaultAsync(c => c.Id == conversationId 
                && (c.User1Id == userId || c.User2Id == userId));

        if (conversation == null) return;

        var sender = conversation.User1Id == userId ? conversation.User1 : conversation.User2;
        var receiverId = conversation.User1Id == userId ? conversation.User2Id : conversation.User1Id;

        // Save message to database
        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = userId,
            Content = content.Trim(),
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync();

        // Create DTO for clients
        var messageDto = new ChatMessageDto(
            message.Id,
            conversationId,
            userId,
            sender.FullName,
            sender.Role,
            content.Trim(),
            message.SentAt,
            false
        );

        // Send to conversation group
        await Clients.Group($"conversation_{conversationId}").SendAsync("ReceiveMessage", messageDto);

        // Also notify the receiver directly if they're online (for unread badge update)
        lock (_lock)
        {
            if (_userConnections.ContainsKey(receiverId))
            {
                foreach (var connectionId in _userConnections[receiverId])
                {
                    Clients.Client(connectionId).SendAsync("NewMessageNotification", conversationId, sender.FullName);
                }
            }
        }

        Console.WriteLine($"[ChatHub] Message sent in conversation {conversationId} by {userId}");
    }

    public async Task MarkAsRead(Guid conversationId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return;

        var messages = await _dbContext.Messages
            .Where(m => m.ConversationId == conversationId 
                && m.SenderId != userId 
                && !m.IsRead)
            .ToListAsync();

        foreach (var msg in messages)
            msg.IsRead = true;

        await _dbContext.SaveChangesAsync();

        // Notify the sender that messages were read
        await Clients.Group($"conversation_{conversationId}").SendAsync("MessagesRead", conversationId, userId);
    }

    public async Task StartTyping(Guid conversationId)
    {
        var userId = GetUserId();
        await Clients.OthersInGroup($"conversation_{conversationId}").SendAsync("UserTyping", conversationId, userId);
    }

    public async Task StopTyping(Guid conversationId)
    {
        var userId = GetUserId();
        await Clients.OthersInGroup($"conversation_{conversationId}").SendAsync("UserStoppedTyping", conversationId, userId);
    }

    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? Context.User?.FindFirst("sub")?.Value;
        
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        
        return Guid.Empty;
    }
}

public record ChatMessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string SenderName,
    string? SenderRole,
    string Content,
    DateTime SentAt,
    bool IsRead
);
