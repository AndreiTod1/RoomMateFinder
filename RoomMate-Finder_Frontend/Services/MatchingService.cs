using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using RoomMate_Finder_Frontend.Models;

namespace RoomMate_Finder_Frontend.Services;

public class MatchingService : IMatchingService
{
    private readonly HttpClient _http;

    public MatchingService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<MatchProfileDto>> GetDiscoverProfilesAsync(Guid userId)
    {
        var res = await _http.GetFromJsonAsync<List<MatchProfileDto>>($"/matching/matches/{userId}");
        return res ?? new List<MatchProfileDto>();
    }

    public async Task<CompatibilityDto> CalculateCompatibilityAsync(Guid userId, Guid otherUserId)
    {
        var res = await _http.GetFromJsonAsync<CompatibilityDto>($"/matching/compatibility/{userId}/{otherUserId}");
        return res!;
    }

    public async Task<List<UserMatchDto>> GetMyMatchesAsync(Guid userId)
    {
        var res = await _http.GetFromJsonAsync<List<UserMatchDto>>($"/matching/my-matches/{userId}");
        return res ?? new List<UserMatchDto>();
    }

    public async Task<LikeResponseDto> LikeProfileAsync(Guid userId, Guid targetUserId)
    {
        var request = new { UserId = userId, TargetUserId = targetUserId };
        var resp = await _http.PostAsJsonAsync("/matching/like", request);
        return await resp.Content.ReadFromJsonAsync<LikeResponseDto>() ?? new LikeResponseDto(false, "Error");
    }

    public async Task<PassResponseDto> PassProfileAsync(Guid userId, Guid targetUserId)
    {
        var request = new { UserId = userId, TargetUserId = targetUserId };
        var resp = await _http.PostAsJsonAsync("/matching/pass", request);
        return await resp.Content.ReadFromJsonAsync<PassResponseDto>() ?? new PassResponseDto(false, "Error");
    }
}
