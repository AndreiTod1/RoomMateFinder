namespace RoomMate_Finder_Frontend.Services;

public interface IRoommateService
{
    // User endpoints
    Task<SendRoommateRequestResponse?> SendRoommateRequestAsync(Guid targetUserId, string? message);
    Task<MyRoommateRequestsResponse?> GetMyRequestsAsync();
    
    // Admin endpoints
    Task<List<PendingRoommateRequestDto>> GetPendingRequestsAsync();
    Task<ApproveRequestResponse?> ApproveRequestAsync(Guid requestId);
    Task<RejectRequestResponse?> RejectRequestAsync(Guid requestId);
    Task<List<RoommateRelationshipDto>> GetRelationshipsAsync();
    Task<DeleteRelationshipResponse?> DeleteRelationshipAsync(Guid relationshipId);
}

public record SendRoommateRequestResponse(Guid Id, string Message);

public record MyRoommateRequestsResponse(
    List<MyRequestDto> SentRequests,
    List<MyRequestDto> ReceivedRequests,
    List<MyRoommateDto> ActiveRoommates
);

public record MyRequestDto(
    Guid Id,
    Guid OtherUserId,
    string OtherUserName,
    string OtherUserEmail,
    string? Message,
    string Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt
);

public record MyRoommateDto(
    Guid RelationshipId,
    Guid RoommateId,
    string RoommateName,
    string RoommateEmail,
    DateTime Since
);

public record PendingRoommateRequestDto(
    Guid Id,
    Guid RequesterId,
    string RequesterName,
    string RequesterEmail,
    Guid TargetUserId,
    string TargetUserName,
    string TargetUserEmail,
    string? Message,
    DateTime CreatedAt
);

public record ApproveRequestResponse(Guid RelationshipId, string Message);

public record RejectRequestResponse(string Message);

public record RoommateRelationshipDto(
    Guid Id,
    Guid User1Id,
    string User1Name,
    string User1Email,
    Guid User2Id,
    string User2Name,
    string User2Email,
    string ApprovedByAdminName,
    DateTime CreatedAt,
    bool IsActive
);

public record DeleteRelationshipResponse(string Message);

