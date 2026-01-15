using Microsoft.AspNetCore.SignalR.Client;

namespace RoomMate_Finder_Frontend.Services.Internals;

public class SignalRHubConnection : IHubConnection
{
    private readonly HubConnection _hubConnection;

    public SignalRHubConnection(HubConnection hubConnection)
    {
        _hubConnection = hubConnection;
    }

    public HubConnectionState State => _hubConnection.State;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        return _hubConnection.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return _hubConnection.StopAsync(cancellationToken);
    }

    public Task InvokeAsync(string methodName, object? arg1, CancellationToken cancellationToken = default)
    {
        return _hubConnection.InvokeAsync(methodName, arg1, cancellationToken);
    }

    public Task InvokeAsync(string methodName, object? arg1, object? arg2, CancellationToken cancellationToken = default)
    {
        return _hubConnection.InvokeAsync(methodName, arg1, arg2, cancellationToken);
    }

    public Task InvokeAsync(string methodName, CancellationToken cancellationToken = default)
    {
        return _hubConnection.InvokeAsync(methodName, cancellationToken);
    }

    public IDisposable On<T1>(string methodName, Action<T1> handler)
    {
        return _hubConnection.On(methodName, handler);
    }

    public IDisposable On<T1, T2>(string methodName, Action<T1, T2> handler)
    {
        return _hubConnection.On(methodName, handler);
    }

    public ValueTask DisposeAsync()
    {
        return _hubConnection.DisposeAsync();
    }
}
