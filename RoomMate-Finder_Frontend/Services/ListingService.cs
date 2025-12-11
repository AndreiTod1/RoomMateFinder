using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace RoomMate_Finder_Frontend.Services;

public class ListingService : IListingService
{
    private readonly HttpClient _http;

    public ListingService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ListingsResponse> SearchAsync(ListingsSearchRequest request)
    {
        // Use POST /room-listings/search as per backend
        var resp = await _http.PostAsJsonAsync("/room-listings/search", request);
        if (!resp.IsSuccessStatusCode)
        {
            return new ListingsResponse(new List<ListingSummaryDto>(), 0, request.Page, request.PageSize);
        }
        var result = await resp.Content.ReadFromJsonAsync<ListingsResponse>();
        return result ?? new ListingsResponse(new List<ListingSummaryDto>(), 0, request.Page, request.PageSize);
    }

    public async Task<ListingDto?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _http.GetFromJsonAsync<ListingDto>($"/room-listings/{id}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<ListingDto> CreateAsync(CreateListingRequest request)
    {
        var resp = await _http.PostAsJsonAsync("/room-listings", request);
        if (!resp.IsSuccessStatusCode)
        {
            var error = await resp.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create listing: {error}");
        }
        var result = await resp.Content.ReadFromJsonAsync<ListingDto>();
        return result!;
    }

    public async Task<ListingDto> UpdateAsync(Guid id, UpdateListingRequest request)
    {
        var resp = await _http.PutAsJsonAsync($"/room-listings/{id}", request);
        if (!resp.IsSuccessStatusCode)
        {
            var error = await resp.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to update listing: {error}");
        }
        var result = await resp.Content.ReadFromJsonAsync<ListingDto>();
        return result!;
    }

    public async Task DeleteAsync(Guid id)
    {
        var resp = await _http.DeleteAsync($"/room-listings/{id}");
        if (!resp.IsSuccessStatusCode)
        {
            var error = await resp.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to delete listing: {error}");
        }
    }
}
