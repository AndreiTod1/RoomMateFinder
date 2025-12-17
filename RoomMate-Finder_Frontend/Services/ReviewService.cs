using RoomMate_Finder_Frontend.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace RoomMate_Finder_Frontend.Services
{
    public class ReviewService : IReviewService
    {
        private readonly HttpClient _httpClient;

        public ReviewService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<UserMatchDto>> GetMatchesForReview(Guid userId)
        {
            var res = await _httpClient.GetFromJsonAsync<List<UserMatchDto>>($"/matching/my-matches/{userId}");
            return res ?? new List<UserMatchDto>();
        }

        public async Task LeaveReviewAsync(string reviewedUserId, int rating, string comment)
        {
            var reviewData = new { rating, comment };
            var response = await _httpClient.PostAsJsonAsync($"/profiles/{reviewedUserId}/reviews", reviewData);
            response.EnsureSuccessStatusCode();
        }
    }
}
