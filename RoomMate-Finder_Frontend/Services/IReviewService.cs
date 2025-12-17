using RoomMate_Finder_Frontend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoomMate_Finder_Frontend.Services
{
    public interface IReviewService
    {
        Task<IEnumerable<UserMatchDto>> GetMatchesForReview(Guid userId);
        Task LeaveReviewAsync(string reviewedUserId, int rating, string comment);
    }
}
