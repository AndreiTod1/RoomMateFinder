using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace RoomMate_Finder_Frontend.Services;

public class ApiAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private readonly HttpClient _http;
    private const string TokenKey = "authToken";

    public ApiAuthenticationStateProvider(IJSRuntime js, HttpClient http)
    {
        _js = js;
        _http = http;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _js.InvokeAsync<string>("localStorage.getItem", TokenKey);
            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public Task MarkUserAsAuthenticated(string token)
    {
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        return Task.CompletedTask;
    }

    public Task MarkUserAsLoggedOut()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
        return Task.CompletedTask;
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var parts = jwt.Split('.');
        if (parts.Length < 2) return claims;

        var payload = parts[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);
        if (keyValuePairs == null) return claims;

        ExtractClaimsFromJson(claims, keyValuePairs);
        MapLegacyClaims(claims);

        return claims;
    }

    private static void ExtractClaimsFromJson(List<Claim> claims, Dictionary<string, JsonElement> keyValuePairs)
    {
        foreach (var kv in keyValuePairs)
        {
            if (kv.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in kv.Value.EnumerateArray())
                {
                    claims.Add(new Claim(kv.Key, item.ToString()));
                }
            }
            else
            {
                claims.Add(new Claim(kv.Key, kv.Value.ToString()));
            }
        }
    }

    private static void MapLegacyClaims(List<Claim> claims)
    {
        AddClaimIfNotPresent(claims, "name", ClaimTypes.Name);
        AddClaimIfNotPresent(claims, "sub", ClaimTypes.NameIdentifier);
        AddClaimIfNotPresent(claims, "role", ClaimTypes.Role);
        AddClaimIfNotPresent(claims, "Role", ClaimTypes.Role);
    }

    private static void AddClaimIfNotPresent(List<Claim> claims, string sourceKey, string targetType)
    {
        if (!claims.Any(c => c.Type == targetType))
        {
            var source = claims.FirstOrDefault(c => c.Type == sourceKey);
            if (source != null)
            {
                claims.Add(new Claim(targetType, source.Value));
            }
        }
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        base64 = base64.Replace('-', '+').Replace('_', '/');
        return Convert.FromBase64String(base64);
    }
}
