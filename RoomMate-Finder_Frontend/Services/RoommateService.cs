using System.Net.Http.Json;
using System.Text.Json;

namespace RoomMate_Finder_Frontend.Services;

public class RoommateService : IRoommateService
{
    private readonly HttpClient _http;

    public RoommateService(HttpClient http)
    {
        _http = http;
    }

    // User endpoints
    public async Task<SendRoommateRequestResponse?> SendRoommateRequestAsync(Guid targetUserId, string? message)
    {
        var response = await _http.PostAsJsonAsync("api/roommates/requests", new { TargetUserId = targetUserId, Message = message });
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<SendRoommateRequestResponse>();
        }
        var errorMessage = await ParseErrorMessage(response);
        throw new Exception(errorMessage);
    }

    public async Task<MyRoommateRequestsResponse?> GetMyRequestsAsync()
    {
        return await _http.GetFromJsonAsync<MyRoommateRequestsResponse>("api/roommates/my-requests");
    }

    public async Task<UserRoommateDto?> GetUserRoommateAsync(Guid userId)
    {
        return await _http.GetFromJsonAsync<UserRoommateDto?>($"api/roommates/user/{userId}");
    }

    // Admin endpoints
    public async Task<List<PendingRoommateRequestDto>> GetPendingRequestsAsync()
    {
        return await _http.GetFromJsonAsync<List<PendingRoommateRequestDto>>("api/roommates/requests/pending") ?? new();
    }

    public async Task<ApproveRequestResponse?> ApproveRequestAsync(Guid requestId)
    {
        var response = await _http.PostAsync($"api/roommates/requests/{requestId}/approve", null);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ApproveRequestResponse>();
        }
        var errorMessage = await ParseErrorMessage(response);
        throw new Exception(errorMessage);
    }

    public async Task<RejectRequestResponse?> RejectRequestAsync(Guid requestId)
    {
        var response = await _http.PostAsync($"api/roommates/requests/{requestId}/reject", null);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<RejectRequestResponse>();
        }
        var errorMessage = await ParseErrorMessage(response);
        throw new Exception(errorMessage);
    }

    public async Task<List<RoommateRelationshipDto>> GetRelationshipsAsync()
    {
        return await _http.GetFromJsonAsync<List<RoommateRelationshipDto>>("api/roommates/relationships") ?? new();
    }

    public async Task<DeleteRelationshipResponse?> DeleteRelationshipAsync(Guid relationshipId)
    {
        var response = await _http.DeleteAsync($"api/roommates/relationships/{relationshipId}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<DeleteRelationshipResponse>();
        }
        var errorMessage = await ParseErrorMessage(response);
        throw new Exception(errorMessage);
    }

    private async Task<string> ParseErrorMessage(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                return $"Eroare de la server: {response.StatusCode}";
            }

            // Try to parse as JSON with "error" property
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("error", out var errorProp))
            {
                return errorProp.GetString() ?? content;
            }
            if (doc.RootElement.TryGetProperty("message", out var messageProp))
            {
                return messageProp.GetString() ?? content;
            }
            if (doc.RootElement.TryGetProperty("title", out var titleProp))
            {
                return titleProp.GetString() ?? content;
            }
            
            return content;
        }
        catch
        {
            return $"Eroare de la server: {response.StatusCode}";
        }
    }
}

