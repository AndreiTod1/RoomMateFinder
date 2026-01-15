using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;

namespace RoomMate_Finder_Frontend.Services;

public class AuthService : IAuthService
{
    private const string TokenKey = "authToken";
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private readonly ApiAuthenticationStateProvider _authStateProvider;

    public AuthService(HttpClient http, IJSRuntime js, ApiAuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _js = js;
        _authStateProvider = authStateProvider;
    }

    public async Task LoginAsync(string email, string password)
    {
        var req = new { Email = email, Password = password };
        var resp = await _http.PostAsJsonAsync("/profiles/login", req);

        if (!resp.IsSuccessStatusCode)
        {
            var message = await resp.Content.ReadAsStringAsync();
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(message) ? "Login failed" : message);
        }

        using var stream = await resp.Content.ReadAsStreamAsync();
        var doc = await JsonDocument.ParseAsync(stream);

        if (!doc.RootElement.TryGetProperty("token", out var tokenEl) && !doc.RootElement.TryGetProperty("Token", out tokenEl))
        {
            throw new InvalidOperationException("Token missing in response");
        }

        var token = tokenEl.GetString();
        if (string.IsNullOrWhiteSpace(token)) throw new InvalidOperationException("Token empty in response");

        await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await _authStateProvider.MarkUserAsAuthenticated(token);
    }

    public async Task LogoutAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        _http.DefaultRequestHeaders.Authorization = null;
        await _authStateProvider.MarkUserAsLoggedOut();
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await _js.InvokeAsync<string>("localStorage.getItem", TokenKey);
        }
        catch
        {
            return null;
        }
    }

    public async Task<RegisterResult> RegisterWithPictureAsync(string email, string password, string fullName, int age, string gender, 
        string university, string bio, string lifestyle, string interests, string? profilePictureUrl)
    {
        var request = new
        {
            email,
            password,
            fullName,
            age,
            gender,
            university,
            bio,
            lifestyle,
            interests,
            profilePictureUrl
        };

        var resp = await _http.PostAsJsonAsync("/profiles", request);

        if (!resp.IsSuccessStatusCode)
        {
            var message = await resp.Content.ReadAsStringAsync();
            return new RegisterResult { Successful = false, Errors = new[] { message } };
        }
        
        return new RegisterResult { Successful = true };
    }
}
