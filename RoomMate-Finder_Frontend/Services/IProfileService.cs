using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using RoomMate_Finder_Frontend.Models;

namespace RoomMate_Finder_Frontend.Services
{
    public interface IProfileService
    {
        Task<List<ProfileDto>> GetAllAsync();
        Task<List<ProfileDto>> GetAdminsAsync();
        Task DeleteProfileAsync(Guid id);
        Task UpdateRoleAsync(Guid id, string role);
        Task<PaginatedUsersResponse> GetAllUsersAsync(int page, int pageSize, string? search);
        Task<ProfileDto?> GetByIdAsync(Guid id);
        Task<ProfileDto?> GetCurrentAsync();
        Task<ProfileDto?> UpdateAsync(Guid id, UpdateProfileRequestDto update, IBrowserFile? profilePictureFile = null);
        Task<IEnumerable<Review>> GetUserReviews(Guid userId);
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
        DateTime CreatedAt,
        string? ProfilePicturePath,
        string Role = "User"
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

    public record PaginatedUsersResponse(
        List<UserDto> Users,
        int TotalCount,
        int Page,
        int PageSize
    );

    public record UserDto(
        Guid Id,
        string Email,
        string FullName,
        int Age,
        string Gender,
        string University,
        string? ProfilePicturePath,
        DateTime CreatedAt,
        string Role
    );
}
