using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoomMate_Finder_Frontend.Services
{
    public interface IProfileService
    {
        Task<List<ProfileDto>> GetAllAsync();
        Task<ProfileDto?> GetByIdAsync(Guid id);
        Task<ProfileDto?> GetCurrentAsync();
        Task<ProfileDto?> UpdateAsync(Guid id, UpdateProfileRequestDto update);
    }

    public record ProfileDto(
        Guid Id,
        string Email,
        string FullName,
        int Age,
        string Gender,
        string University,
        string Bio,
        string Lifestyle,
        string Interests,
        DateTime CreatedAt
    );

    public record UpdateProfileRequestDto(
        string? FullName,
        int? Age,
        string? Gender,
        string? University,
        string? Bio,
        string? Lifestyle,
        string? Interests
    );
}
