using RoomMate_Finder_Frontend.Models;

namespace RoomMate_Finder_Frontend.Services;

public interface IMatchingService
{
    Task<List<MatchProfileDto>> GetDiscoverProfilesAsync(Guid userId);
    Task<CompatibilityDto> CalculateCompatibilityAsync(Guid userId, Guid otherUserId);
    Task<List<UserMatchDto>> GetMyMatchesAsync(Guid userId);
    Task<LikeResponseDto> LikeProfileAsync(Guid userId, Guid targetUserId);
    Task<PassResponseDto> PassProfileAsync(Guid userId, Guid targetUserId);
}

