using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace RoomMate_Finder_Frontend.Services;

public interface IChatService : IAsyncDisposable
{
    event Action<ChatMessageDto>? OnMessageReceived;
    event Action<Guid, string>? OnNewMessageNotification;
    event Action<Guid, Guid>? OnMessagesRead;
    event Action<Guid, Guid>? OnUserTyping;
    event Action<Guid, Guid>? OnUserStoppedTyping;
    
    bool IsConnected { get; }
    Task ConnectAsync(string accessToken);
    Task DisconnectAsync();
    Task JoinConversationAsync(Guid conversationId);
    Task LeaveConversationAsync(Guid conversationId);
    Task SendMessageAsync(Guid conversationId, string content);
    Task MarkAsReadAsync(Guid conversationId);
    Task StartTypingAsync(Guid conversationId);
    Task StopTypingAsync(Guid conversationId);
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

public class ChatService : IChatService
{
    private HubConnection? _hubConnection;
    private readonly string _hubUrl;
    
    public event Action<ChatMessageDto>? OnMessageReceived;
    public event Action<Guid, string>? OnNewMessageNotification;
    public event Action<Guid, Guid>? OnMessagesRead;
    public event Action<Guid, Guid>? OnUserTyping;
    public event Action<Guid, Guid>? OnUserStoppedTyping;
    
    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public ChatService(IConfiguration configuration)
    {
        var baseUrl = configuration["ApiBaseUrl"] ?? "http://localhost:5111";
        _hubUrl = $"{baseUrl}/hubs/chat";
    }

    public async Task ConnectAsync(string accessToken)
    {
        if (_hubConnection != null)
        {
            await DisconnectAsync();
        }

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(accessToken);
            })
            .WithAutomaticReconnect()
            .Build();

        // Register event handlers
        _hubConnection.On<ChatMessageDto>("ReceiveMessage", message =>
        {
            OnMessageReceived?.Invoke(message);
        });

        _hubConnection.On<Guid, string>("NewMessageNotification", (conversationId, senderName) =>
        {
            OnNewMessageNotification?.Invoke(conversationId, senderName);
        });

        _hubConnection.On<Guid, Guid>("MessagesRead", (conversationId, userId) =>
        {
            OnMessagesRead?.Invoke(conversationId, userId);
        });

        _hubConnection.On<Guid, Guid>("UserTyping", (conversationId, userId) =>
        {
            Console.WriteLine($"[ChatService] Received UserTyping: conv={conversationId}, user={userId}");
            OnUserTyping?.Invoke(conversationId, userId);
        });

        _hubConnection.On<Guid, Guid>("UserStoppedTyping", (conversationId, userId) =>
        {
            Console.WriteLine($"[ChatService] Received UserStoppedTyping: conv={conversationId}, user={userId}");
            OnUserStoppedTyping?.Invoke(conversationId, userId);
        });

        try
        {
            await _hubConnection.StartAsync();
            Console.WriteLine($"[ChatService] Connected to SignalR hub");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatService] Connection failed: {ex.Message}");
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
            Console.WriteLine("[ChatService] Disconnected from SignalR hub");
        }
    }

    public async Task JoinConversationAsync(Guid conversationId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("JoinConversation", conversationId);
        }
    }

    public async Task LeaveConversationAsync(Guid conversationId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("LeaveConversation", conversationId);
        }
    }

    public async Task SendMessageAsync(Guid conversationId, string content)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("SendMessage", conversationId, content);
        }
    }

    public async Task MarkAsReadAsync(Guid conversationId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("MarkAsRead", conversationId);
        }
    }

    public async Task StartTypingAsync(Guid conversationId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("StartTyping", conversationId);
        }
    }

    public async Task StopTypingAsync(Guid conversationId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("StopTyping", conversationId);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}
