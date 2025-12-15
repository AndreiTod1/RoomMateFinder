using System.Net.Http.Json;

namespace RoomMate_Finder_Frontend.Services;

public class ConversationService : IConversationService
{
    private readonly HttpClient _http;

    public ConversationService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ConversationDto>> GetConversationsAsync()
    {
        try
        {
            var response = await _http.GetAsync("/conversations");
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<GetConversationsResponse>();
                return result?.Conversations ?? new List<ConversationDto>();
            }

            return new List<ConversationDto>();
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception)
        {
            return new List<ConversationDto>();
        }
    }

    public async Task<List<MessageDto>> GetMessagesAsync(Guid conversationId)
    {
        try
        {
            var response = await _http.GetAsync($"/conversations/{conversationId}/messages");
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<GetMessagesResponse>();
                return result?.Messages ?? new List<MessageDto>();
            }

            return new List<MessageDto>();
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception)
        {
            return new List<MessageDto>();
        }
    }

    public async Task<ConversationDto?> StartConversationAsync(Guid otherUserId)
    {
        try
        {
            var request = new StartConversationRequest(otherUserId);
            var response = await _http.PostAsJsonAsync("/conversations", request);
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<StartConversationResponse>();
                if (result != null)
                {
                    return new ConversationDto(
                        result.ConversationId,
                        otherUserId,
                        "", // Name will be populated by the UI
                        null, // ProfilePicture
                        null, // Role
                        DateTime.UtcNow
                    );
                }
            }

            return null;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<MessageDto?> SendMessageAsync(Guid conversationId, string content)
    {
        try
        {
            var request = new SendMessageRequest(conversationId, content);
            var response = await _http.PostAsJsonAsync($"/conversations/{conversationId}/messages", request);
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SendMessageResponse>();
                if (result != null)
                {
                    return new MessageDto(
                        result.MessageId,
                        Guid.Empty, // Will be the current user
                        "",
                        null, // SenderRole will be populated on refresh
                        content,
                        DateTime.UtcNow,
                        false
                    );
                }
            }

            return null;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task MarkMessagesAsReadAsync(Guid conversationId)
    {
        try
        {
            var request = new MarkMessagesAsReadRequest(conversationId);
            await _http.PutAsJsonAsync($"/conversations/{conversationId}/read", request);
        }
        catch (Exception)
        {
            // Silently fail
        }
    }

    // Response DTOs
    private record GetConversationsResponse(List<ConversationDto> Conversations);
    private record GetMessagesResponse(List<MessageDto> Messages);
    private record StartConversationRequest(Guid OtherUserId);
    private record StartConversationResponse(Guid ConversationId);
    private record SendMessageRequest(Guid ConversationId, string Content);
    private record SendMessageResponse(Guid MessageId);
    private record MarkMessagesAsReadRequest(Guid ConversationId);
}

