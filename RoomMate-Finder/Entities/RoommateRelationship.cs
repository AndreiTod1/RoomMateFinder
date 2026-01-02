namespace RoomMate_Finder.Entities;
public class RoommateRelationship
{
    public Guid Id { get; set; }
    public Guid User1Id { get; set; }
    public Guid User2Id { get; set; }
    public Guid ApprovedByAdminId { get; set; }
    public Guid? OriginalRequestId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public Profile User1 { get; set; } = null!;
    public Profile User2 { get; set; } = null!;
    public Profile ApprovedByAdmin { get; set; } = null!;
    public RoommateRequest? OriginalRequest { get; set; }
}
