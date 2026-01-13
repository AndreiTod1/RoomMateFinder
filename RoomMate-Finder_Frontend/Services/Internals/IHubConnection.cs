using Microsoft.AspNetCore.SignalR.Client;

namespace RoomMate_Finder_Frontend.Services.Internals;

public interface IHubConnection : IAsyncDisposable
{
    HubConnectionState State { get; }
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    
    Task InvokeAsync(string methodName, object? arg1, CancellationToken cancellationToken = default);
    Task InvokeAsync(string methodName, object? arg1, object? arg2, CancellationToken cancellationToken = default);
    Task InvokeAsync(string methodName, CancellationToken cancellationToken = default);
    
    IDisposable On<T1>(string methodName, Action<T1> handler);
    IDisposable On<T1, T2>(string methodName, Action<T1, T2> handler);
}
