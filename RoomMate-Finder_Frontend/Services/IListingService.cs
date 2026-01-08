using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;

namespace RoomMate_Finder_Frontend.Services;

public enum ListingApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public interface IListingService
{
    Task<ListingsResponse> SearchAsync(ListingsSearchRequest request);
    Task<ListingDto?> GetByIdAsync(Guid id);
    Task<ListingDto> CreateAsync(CreateListingRequest request);
    Task<ListingDto> UpdateAsync(Guid id, UpdateListingRequest request);
    Task DeleteAsync(Guid id);
    Task<(bool Success, string Message)> ApproveAsync(Guid id);
    Task<(bool Success, string Message)> RejectAsync(Guid id, string reason);
}

public record ListingsSearchRequest(
    string? City = null,
    string? Area = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    Guid? OwnerId = null,
    bool IncludeInactive = false,
    ListingApprovalStatus? ApprovalStatus = null,
    bool IncludePending = false,
    int Page = 1,
    int PageSize = 12
);

public record ListingsResponse(
    List<ListingSummaryDto> Listings,
    int TotalCount,
    int Page,
    int PageSize
);

public record ListingSummaryDto(
    Guid Id,
    Guid OwnerId,
    string OwnerFullName,
    string Title,
    string City,
    string Area,
    decimal Price,
    DateTime AvailableFrom,
    List<string> Amenities,
    bool IsActive,
    string? ThumbnailPath = null,
    ListingApprovalStatus ApprovalStatus = ListingApprovalStatus.Pending,
    string? RejectionReason = null
);

public record ListingDto(
    Guid Id,
    Guid OwnerId,
    string Title,
    string Description,
    string City,
    string Area,
    decimal Price,
    DateTime AvailableFrom,
    List<string> Amenities,
    DateTime CreatedAt,
    bool IsActive,
    List<string>? ImagePaths = null,
    string? OwnerFullName = null,
    ListingApprovalStatus ApprovalStatus = ListingApprovalStatus.Pending,
    string? RejectionReason = null
);

public record CreateListingRequest(
    string Title,
    string Description,
    string City,
    string Area,
    decimal Price,
    DateTime AvailableFrom,
    List<string> Amenities,
    Guid OwnerId,
    List<IBrowserFile>? Images = null
);

public record UpdateListingRequest(
    string Title,
    string Description,
    string City,
    string Area,
    decimal Price,
    DateTime AvailableFrom,
    List<string> Amenities,
    bool IsActive
);
