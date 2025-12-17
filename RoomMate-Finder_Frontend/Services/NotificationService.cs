using Microsoft.JSInterop;

namespace RoomMate_Finder_Frontend.Services;

public interface INotificationService
{
    event Action? OnNotificationsChanged;
    int UnreadConversationsCount { get; }
    HashSet<Guid> UnreadConversations { get; }
    Task InitializeAsync();
    Task SyncFromServerAsync(IEnumerable<Guid> unreadConversationIds);
    Task AddUnreadConversationAsync(Guid conversationId);
    Task MarkConversationAsReadAsync(Guid conversationId);
    Task ClearAllAsync();
    bool HasUnreadMessages(Guid conversationId);
}

public class NotificationService : INotificationService
{
    private readonly IJSRuntime _jsRuntime;
    private HashSet<Guid> _unreadConversations = new();
    private const string StorageKey = "unread_conversations";
    private bool _initialized = false;

    public event Action? OnNotificationsChanged;
    
    public int UnreadConversationsCount => _unreadConversations.Count;
    public HashSet<Guid> UnreadConversations => _unreadConversations;

    public NotificationService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        
        try
        {
            var stored = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrEmpty(stored))
            {
                var ids = stored.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var id in ids)
                {
                    if (Guid.TryParse(id, out var guid))
                    {
                        _unreadConversations.Add(guid);
                    }
                }
            }
            _initialized = true;
            Console.WriteLine($"[NotificationService] Initialized with {_unreadConversations.Count} unread conversations");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationService] Error initializing: {ex.Message}");
            _initialized = true;
        }
    }

    public async Task SyncFromServerAsync(IEnumerable<Guid> unreadConversationIds)
    {
        _unreadConversations = new HashSet<Guid>(unreadConversationIds);
        await SaveToStorageAsync();
        OnNotificationsChanged?.Invoke();
        Console.WriteLine($"[NotificationService] Synced from server: {_unreadConversations.Count} unread conversations");
    }

    public async Task AddUnreadConversationAsync(Guid conversationId)
    {
        if (_unreadConversations.Add(conversationId))
        {
            await SaveToStorageAsync();
            OnNotificationsChanged?.Invoke();
            Console.WriteLine($"[NotificationService] Added unread conversation: {conversationId}");
        }
    }

    public async Task MarkConversationAsReadAsync(Guid conversationId)
    {
        if (_unreadConversations.Remove(conversationId))
        {
            await SaveToStorageAsync();
            OnNotificationsChanged?.Invoke();
            Console.WriteLine($"[NotificationService] Marked as read: {conversationId}");
        }
    }

    public async Task ClearAllAsync()
    {
        _unreadConversations.Clear();
        await SaveToStorageAsync();
        OnNotificationsChanged?.Invoke();
    }

    public bool HasUnreadMessages(Guid conversationId)
    {
        return _unreadConversations.Contains(conversationId);
    }

    private async Task SaveToStorageAsync()
    {
        try
        {
            var value = string.Join(",", _unreadConversations);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, value);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[NotificationService] Error saving: {ex.Message}");
        }
    }
}
