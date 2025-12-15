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
        using var content = new MultipartFormDataContent();
        
        content.Add(new StringContent(request.Title), "Title");
        content.Add(new StringContent(request.Description), "Description");
        content.Add(new StringContent(request.City), "City");
        content.Add(new StringContent(request.Area), "Area");
        content.Add(new StringContent(request.Price.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Price");
        content.Add(new StringContent(request.AvailableFrom.ToString("o")), "AvailableFrom");
        content.Add(new StringContent(string.Join(",", request.Amenities)), "Amenities");
        
        // Add images if present
        if (request.Images != null && request.Images.Count > 0)
        {
            Console.WriteLine($"[ListingService] Uploading {request.Images.Count} images");
            foreach (var image in request.Images)
            {
                // Copy to MemoryStream to ensure stream is read correctly
                using var sourceStream = image.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
                var memoryStream = new MemoryStream();
                await sourceStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                
                Console.WriteLine($"[ListingService] Image: {image.Name}, Size: {memoryStream.Length} bytes, ContentType: {image.ContentType}");
                
                var fileContent = new StreamContent(memoryStream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType);
                content.Add(fileContent, "Images", image.Name);
            }
        }
        else
        {
            Console.WriteLine("[ListingService] No images to upload");
        }
        
        var resp = await _http.PostAsync("/room-listings", content);
        if (!resp.IsSuccessStatusCode)
        {
            var error = await resp.Content.ReadAsStringAsync();
            Console.WriteLine($"[ListingService] Upload failed: {error}");
            throw new InvalidOperationException($"Failed to create listing: {error}");
        }
        var result = await resp.Content.ReadFromJsonAsync<ListingDto>();
        Console.WriteLine($"[ListingService] Upload successful, listing ID: {result?.Id}");
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
