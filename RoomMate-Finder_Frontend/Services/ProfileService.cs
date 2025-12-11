using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;

namespace RoomMate_Finder_Frontend.Services;

public class ProfileService : IProfileService
{
    private readonly HttpClient _http;

    public ProfileService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ProfileDto>> GetAdminsAsync()
    {
        var resp = await _http.GetFromJsonAsync<List<ProfileDto>>("/api/admins");
        return resp ?? new List<ProfileDto>();
    }

    public async Task DeleteProfileAsync(Guid id)
    {
        var resp = await _http.DeleteAsync($"/api/admins/users/{id}");
        if (!resp.IsSuccessStatusCode)
        {
            var error = await resp.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Delete failed: {error}");
        }
    }

    public async Task UpdateRoleAsync(Guid id, string role)
    {
        var resp = await _http.PutAsJsonAsync($"/api/admins/users/{id}/role", role);
        if (!resp.IsSuccessStatusCode)
        {
            var error = await resp.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Update role failed: {error}");
        }
    }

    public async Task<PaginatedUsersResponse> GetAllUsersAsync(int page, int pageSize, string? search)
    {
        var url = $"/api/admins/users?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"&search={Uri.EscapeDataString(search)}";
        }
        var resp = await _http.GetFromJsonAsync<PaginatedUsersResponse>(url);
        return resp ?? new PaginatedUsersResponse(new List<UserDto>(), 0, page, pageSize);
    }

    public async Task<List<ProfileDto>> GetAllAsync()
    {
        var resp = await _http.GetFromJsonAsync<List<ProfileDto>>("/profiles");
        return resp ?? new List<ProfileDto>();
    }

    public async Task<ProfileDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var resp = await _http.GetAsync($"/profiles/{id}");
            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }

            if (resp.IsSuccessStatusCode)
            {
                return await resp.Content.ReadFromJsonAsync<ProfileDto>();
            }

            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            var text = await resp.Content.ReadAsStringAsync();
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(text) ? "Failed to get profile" : text);
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (System.Net.Http.HttpRequestException)
        {
            // network error  return null so UI can show loading/empty
            return null;
        }
    }

    public async Task<ProfileDto?> GetCurrentAsync()
    {
        try
        {
            var resp = await _http.GetAsync("/profiles/me");
            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }

            if (resp.IsSuccessStatusCode)
            {
                return await resp.Content.ReadFromJsonAsync<ProfileDto>();
            }

            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            var text = await resp.Content.ReadAsStringAsync();
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(text) ? "Failed to get current profile" : text);
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (System.Net.Http.HttpRequestException)
        {
            return null;
        }
    }

    // Updates the profile for the specified user id. Returns the updated profile on success, otherwise null.
    public async Task<ProfileDto?> UpdateAsync(Guid id, UpdateProfileRequestDto update, IBrowserFile? profilePictureFile = null)
    {
        using var content = new MultipartFormDataContent();

        if (update.FullName != null)
            content.Add(new StringContent(update.FullName), nameof(update.FullName));
        if (update.Age.HasValue)
            content.Add(new StringContent(update.Age.Value.ToString()), nameof(update.Age));
        if (update.Gender != null)
            content.Add(new StringContent(update.Gender), nameof(update.Gender));
        if (update.University != null)
            content.Add(new StringContent(update.University), nameof(update.University));
        if (update.Bio != null)
            content.Add(new StringContent(update.Bio), nameof(update.Bio));
        if (update.Lifestyle != null)
            content.Add(new StringContent(update.Lifestyle), nameof(update.Lifestyle));
        if (update.Interests != null)
            content.Add(new StringContent(update.Interests), nameof(update.Interests));

        if (profilePictureFile != null)
        {
            var stream = profilePictureFile.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(profilePictureFile.ContentType);

            // Field name must match backend's expected form field (ProfilePicture)
            content.Add(fileContent, "ProfilePicture", profilePictureFile.Name);
        }

        var resp = await _http.PutAsync($"/profiles/{id}", content);
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException();
        }

        if (resp.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            throw new InvalidOperationException("Forbidden");
        }

        if (resp.IsSuccessStatusCode)
        {
            try
            {
                return await resp.Content.ReadFromJsonAsync<ProfileDto>();
            }
            catch
            {
                return null;
            }
        }

        var text = await resp.Content.ReadAsStringAsync();
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(text) ? "Update failed" : text);
    }
}
