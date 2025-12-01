namespace RoomMate_Finder.Entities;

public class UserAction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } // Who performed the action
    public Guid TargetUserId { get; set; } // Who was the target
    public ActionType ActionType { get; set; } // Like or Pass
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Profile User { get; set; } = null!;
    public Profile TargetUser { get; set; } = null!;
}

public enum ActionType
{
    Like = 1,
    Pass = 2
}

public class Match
{
    public Guid Id { get; set; }
    public Guid User1Id { get; set; }
    public Guid User2Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Profile User1 { get; set; } = null!;
    public Profile User2 { get; set; } = null!;
}
