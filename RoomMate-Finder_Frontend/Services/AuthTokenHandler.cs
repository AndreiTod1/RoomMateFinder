using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace RoomMate_Finder_Frontend.Services;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly IJSRuntime _js;
    private const string TokenKey = "authToken";

    public AuthTokenHandler(IJSRuntime js)
    {
        _js = js;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _js.InvokeAsync<string>("localStorage.getItem", TokenKey);
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        catch
        {
            // JS interop not available yet, continue without token
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

