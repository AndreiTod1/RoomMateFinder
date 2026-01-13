using Microsoft.AspNetCore.SignalR.Client;
using RoomMate_Finder_Frontend.Services.Internals;

namespace RoomMate_Finder_Frontend.Services;

public interface IHubConnectionFactory
{
    IHubConnection CreateConnection(string url, string accessToken);
}

public class HubConnectionFactory : IHubConnectionFactory
{
    public IHubConnection CreateConnection(string url, string accessToken)
    {
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(url, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(accessToken);
            })
            .WithAutomaticReconnect()
            .Build();
            
        return new SignalRHubConnection(hubConnection);
    }
}
